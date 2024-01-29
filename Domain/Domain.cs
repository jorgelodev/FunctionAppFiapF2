using FluentValidation.Results;

namespace FunctionAppFiapF2.Domain
{
    public class Pedido
    {
        public Pedido()
        {
            ValidationResult = new ValidationResult();
        }
        public int Id { get; set; }
        public string CodigoProduto { get; set; }
        public virtual Produto Produto { get; set; }
        public int? Quantidade { get; set; }
        public string NumeroCartaoCredito { get; set; }
        public string NomeCliente { get; set; }
        public DateTime Data { get; set; }
        public bool Aprovado { get; set; }
        public ValidationResult ValidationResult { get; set; }
        public void DefineProduto(Produto produto)
        {
            Produto = produto;
        }
        public void AprovarPedido()
        {
            Aprovado = true;
            Data = DateTime.Now;
        }
    }

    public class Produto
    {
        public string Codigo { get; set; }
        public string Nome { get; set; }

    }

    public class Estoque
    {
        public string CodigoProduto { get; set; }
        public virtual Produto Produto { get; set; }
        public int Quantidade { get; set; }

        public void DarBaixaEstoque(int quantidade)
        {
            Quantidade -= quantidade;
        }

    }

}
