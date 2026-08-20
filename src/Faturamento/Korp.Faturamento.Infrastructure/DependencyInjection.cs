using Korp.Faturamento.Application.Contracts.Gateways;
using Korp.Faturamento.Infrastructure.Gateways;
using Korp.Faturamento.Infrastructure.Persistence;
using Korp.Faturamento.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Korp.Faturamento.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("FaturamentoDatabase")
            ?? throw new InvalidOperationException("A connection string 'FaturamentoDatabase' deve ser configurada.");

        services.AddDbContext<BillingDbContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        services.AddScoped<IInvoiceNumberGenerator, InvoiceNumberGenerator>();

        string baseUrl = configuration["InventoryApi:BaseUrl"]
            ?? throw new InvalidOperationException("A configuração 'InventoryApi:BaseUrl' deve ser informada.");
        int timeoutSeconds = configuration.GetValue("InventoryApi:TimeoutSeconds", 10);

        services.AddHttpClient<IInventoryGateway, InventoryHttpGateway>(client =>
        {
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
        });

        return services;
    }
}
