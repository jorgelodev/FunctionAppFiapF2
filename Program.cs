using FunctionAppFiapF2.Data.Context;
using FunctionAppFiapF2.Domain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices((hostContext, services) =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        services.Configure<IISServerOptions>(options =>
        {
            options.AllowSynchronousIO = true;
        });
        services.Configure<KestrelServerOptions>(options =>
        {
            options.AllowSynchronousIO = true;
        });

        string connectionString = "Data Source=localhost;Initial Catalog=FunctionApp;Integrated Security=True;Connect Timeout=30;Encrypt=False;";


        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(connectionString);
            options.EnableSensitiveDataLogging(false); 
            options.UseLoggerFactory(LoggerFactory.Create(builder => builder.AddFilter(DbLoggerCategory.Database.Command.Name, LogLevel.None)));
        });


    })
    .Build();

#region Criação de Produtos no Banco
using (var scope = host.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var dbContext = services.GetRequiredService<ApplicationDbContext>();

        CadastraProdutosEEstoque(dbContext);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Ocorreu um erro: {ex.Message}");
    }
}

static void CadastraProdutosEEstoque(ApplicationDbContext dbContext)
{
    if (!dbContext.Produtos.Any())
    {
        Console.WriteLine("Iniciando cadastro dos registros");



        var produto1 = new Produto { Codigo = "P001", Nome = "Camisa" };
        var produto2 = new Produto { Codigo = "P002", Nome = "Calça" };
        var produto3 = new Produto { Codigo = "P003", Nome = "Meia" };
        var produto4 = new Produto { Codigo = "P004", Nome = "Luva" };

        dbContext.Produtos.Add(produto1);
        dbContext.Produtos.Add(produto2);
        dbContext.Produtos.Add(produto3);
        dbContext.Produtos.Add(produto4);


        var estoque1 = new Estoque { CodigoProduto = "P001", Quantidade = 10 };
        var estoque2 = new Estoque { CodigoProduto = "P002", Quantidade = 10 };
        var estoque3 = new Estoque { CodigoProduto = "P003", Quantidade = 10 };
        var estoque4 = new Estoque { CodigoProduto = "P004", Quantidade = 10 };

        dbContext.Estoque.Add(estoque1);
        dbContext.Estoque.Add(estoque2);
        dbContext.Estoque.Add(estoque3);
        dbContext.Estoque.Add(estoque4);

        dbContext.SaveChanges();

        Console.WriteLine("Produtos: ");
        Console.WriteLine("");
        foreach (var produto in dbContext.Produtos)
        {   
            Console.WriteLine($"Codigo: {produto.Codigo}\t Nome: {produto.Nome}");
        }

        Console.WriteLine("Estoque: ");
        Console.WriteLine("");
        foreach (var estoque in dbContext.Estoque)
        {
            Console.WriteLine($"Codigo Produto: {estoque.CodigoProduto}\t Quantidade: {estoque.Quantidade}");
        }

        Console.WriteLine("Produtos e Estoque cadastrados com sucesso!");
    }
}
#endregion
host.Run();
