using FluentValidation.Results;
using FunctionAppFiapF2.Data.Context;
using FunctionAppFiapF2.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net;

namespace FunctionAppFiapF2
{
    /// <summary>
    /// Function do Tipo Durable. Utilizando o Pattern Function Chaining.
    /// </summary>
    public class FunctionAppFiap
    {
        [Function("PedidoOrchestrator")]
        public async Task<IActionResult> RunOrchestrator(
       [OrchestrationTrigger] TaskOrchestrationContext context,
       FunctionContext functionContext)
        {
            ILogger log = context.CreateReplaySafeLogger("PedidoOrchestrator");

            var pedido = context.GetInput<Pedido>();

            try
            {
                var pedidoAprovado = await context.CallActivityAsync<Pedido>("AprovarPedido", pedido);

                var pedidoProcessado = await context.CallActivityAsync<Pedido>("ProcessarPedido", pedidoAprovado);

                if (pedidoProcessado.Aprovado)
                    return new OkObjectResult(pedidoProcessado);
                else
                    return new BadRequestObjectResult(pedidoProcessado);
            }
            catch (Exception ex)
            {
                log.LogError($"Erro durante a execução: {ex.Message}");
                return new BadRequestObjectResult(new { Status = $"Erro durante a execução: {ex.Message}" });
            }
        }

        [Function("AprovarPedido")]
        public Pedido AprovarPedido(
       [ActivityTrigger] Pedido pedido,
        FunctionContext context)
        {
            ILogger log = context.GetLogger("AprovarPedido");

            var produto = _dbContext.Produtos.Where(p => p.Codigo == pedido.CodigoProduto).FirstOrDefault();

            pedido.DefineProduto(produto);

            log.LogInformation($"Aprovando pedido para {pedido.Quantidade} unidades do produto {pedido.Produto.Codigo} - {pedido.Produto.Nome}, para {pedido.NomeCliente}");

            if (!ValidarCartaoCredito(pedido.NumeroCartaoCredito, log))
            {
                log.LogInformation($"Pedido recusado. Cartão de crédito inválido.");

                pedido.ValidationResult.Errors.Add(new ValidationFailure("cartaoRecusado", "Cartão Recusado, informe um cartão'com 16 dígitos"));
            }

            if (pedido.Quantidade < 1)
            {
                log.LogInformation($"Pedido recusado. Produto : {pedido.Produto.Codigo} - Cliente: {pedido.NomeCliente}. Motivo: Quantidade informada inválida");

                pedido.ValidationResult.Errors.Add(new ValidationFailure("quantidadeInváida", "Quantidade do pedido inválida."));
            }

            if (pedido.Quantidade > _dbContext.Estoque.Where(p => p.CodigoProduto == pedido.CodigoProduto).FirstOrDefault().Quantidade)
            {
                log.LogInformation($"Pedido recusado. Produto : {pedido.Produto.Codigo} - Cliente: {pedido.NomeCliente}. Motivo: Quantidade do pedido superior a quantidade de produtos disponível");

                pedido.ValidationResult.Errors.Add(new ValidationFailure("quantidadeIndisponível", "Quantidade do pedido superior a quantidade de produtos disponível "));
            }

            if (pedido.ValidationResult.IsValid)
            {
                pedido.AprovarPedido();

                log.LogInformation($"Pedido aprovado. Produto : {pedido.Produto.Codigo} - Cliente: {pedido.NomeCliente}");
            }

            return pedido;
        }

        [Function("ProcessarPedido")]
        public Pedido ProcessarPedido(
       [ActivityTrigger] Pedido pedido,
        FunctionContext context)
        {
            ILogger log = context.GetLogger("ProcessarPedido");

            if (pedido.Aprovado)
            {
                log.LogInformation($"Processando pedido para {pedido.Quantidade} unidades do produto {pedido.Produto.Nome} - {pedido.CodigoProduto} para o cliente {pedido.NomeCliente}");
                
                pedido.Produto = null;

                _dbContext.Pedidos.Add(pedido);
                var estoque = _dbContext.Estoque.Where(e => e.CodigoProduto == pedido.CodigoProduto).First();

                estoque.DarBaixaEstoque((int)pedido.Quantidade);

                try
                {
                    _dbContext.SaveChanges();                    
                }
                catch (Exception ex)
                {
                   
                    log.LogError($"Erro ao salvar Pedido. {pedido.CodigoProduto} para o cliente {pedido.NomeCliente}. Erro: {ex.Message}");

                }
                pedido.DefineProduto(_dbContext.Produtos.Where(p => p.Codigo == pedido.CodigoProduto).First());

                log.LogInformation($"Pedido Processado. Número Pedido: {pedido.Id} - Quantidade: {pedido.Quantidade} - Produto: {pedido.CodigoProduto} - {pedido.Produto.Nome} para o cliente {pedido.NomeCliente}");

            }
            return pedido;
        }

        [Function("HttpStartFunctionApp")]
        public async Task<HttpResponseData> HttpStartFunctionApp(
        [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req,
        [DurableClient] DurableTaskClient client,
        FunctionContext context)
        {
            ILogger log = context.GetLogger("HttpStartFunctionApp");

            Pedido pedidoRequest;
            try
            {
                using (var reader = new StreamReader(req.Body))
                {
                    var requestBody = await reader.ReadToEndAsync();
                    if (string.IsNullOrEmpty(requestBody))
                    {
                        throw new Exception();
                    }

                    pedidoRequest = JsonConvert.DeserializeObject<Pedido>(requestBody);
                }
            }
            catch (Exception)
            {
                log.LogError($"Erro ao recuperar pedido do request. Data: {DateTime.Now}");

                var response = req.CreateResponse(HttpStatusCode.BadRequest);
                await response.WriteStringAsync(JsonConvert.SerializeObject("Informe um JSON válido."));
                return response;
            }

            if (ContemCamposNull(pedidoRequest))
            {
                var response = req.CreateResponse(HttpStatusCode.BadRequest);
                await response.WriteStringAsync(JsonConvert.SerializeObject("Os seguintes campos são obrigatórios: CodigoProduto, NumeroCartaoCredito, Quantidade e NomeCliente."));
                return response;
            }

            if (ProdutoNaoExiste(pedidoRequest.CodigoProduto))
            {
                var response = req.CreateResponse(HttpStatusCode.BadRequest);
                await response.WriteStringAsync(JsonConvert.SerializeObject("Produto não existe."));
                return response;
            }

            string instanceId = await client.ScheduleNewOrchestrationInstanceAsync(
                "PedidoOrchestrator", pedidoRequest);

            log.LogInformation($"Tech Challenge F2 - Iniciado orchestrator com ID = '{instanceId}'.");

            var checkStatusResponse = client.CreateCheckStatusResponse(req, instanceId);
            return checkStatusResponse;
        }


        private readonly ApplicationDbContext _dbContext;

        public FunctionAppFiap(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;

        }

        public static bool ValidarCartaoCredito(string numeroCartaoCredito, ILogger log)
        {
            if (numeroCartaoCredito.Count() == 16)
            {
                log.LogInformation($"Cartão de crédito válido.");
                return true;
            }

            log.LogInformation($"Cartão de crédito inválido. Pedido não aprovado.");

            return false;
        }
        private static bool ContemCamposNull(Pedido pedidoRequest)
        {
            if (pedidoRequest.CodigoProduto != null && pedidoRequest.NumeroCartaoCredito != null && pedidoRequest.NomeCliente != null && pedidoRequest.Quantidade != null)
                return false;
            return true;
        }
        private bool ProdutoNaoExiste(string codigoProduto)
        {
            return !_dbContext.Produtos.Where(p => p.Codigo == codigoProduto).Any();
        }

    }
}
