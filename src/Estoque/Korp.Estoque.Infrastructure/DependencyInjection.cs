using Korp.Estoque.Application.Contracts.Persistence;
using Korp.Estoque.Infrastructure.Persistence;
using Korp.Estoque.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Korp.Estoque.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("EstoqueDatabase")
            ?? throw new InvalidOperationException("A connection string 'EstoqueDatabase' deve ser configurada.");

        services.ConfigureDatabase(connectionString);
        services.ConfigureRepositories();

        return services;
    }

    private static void ConfigureDatabase(this IServiceCollection services, string connectionString) => services.AddDbContext<InventoryDbContext>(options => options.UseSqlServer(connectionString));

    private static void ConfigureRepositories(this IServiceCollection services)
    {
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IStockDebitRepository, StockDebitRepository>();
    }
}
