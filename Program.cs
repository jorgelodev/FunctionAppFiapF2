using FunctionAppFiapF2.Data.Context;
using FunctionAppFiapF2.Domain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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

        //string connectionString = hostContext.Configuration.GetSection("ConnectionString").Value!;

        string connectionString = "Data Source=localhost;Initial Catalog=FunctionApp;Integrated Security=True;Connect Timeout=30;Encrypt=False;";

        services.AddDbContext<ApplicationDbContext>(
            options => SqlServerDbContextOptionsExtensions.UseSqlServer(options, connectionString));

        services.AddDbContext<IApplicationDbContext, ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));
    })
    .Build();

#region Criação de Produtos no Banco
using (var scope = host.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var dbContext = services.GetRequiredService<ApplicationDbContext>();

        // Execute a rotina de verificação e cadastro
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

        Console.WriteLine("Produtos cadastrados com sucesso!");
    }
}
#endregion
host.Run();
