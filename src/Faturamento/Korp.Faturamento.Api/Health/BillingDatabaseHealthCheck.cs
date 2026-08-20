using Korp.Faturamento.Infrastructure.Persistence;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Korp.Faturamento.Api.Health;

public sealed class BillingDatabaseHealthCheck(IServiceScopeFactory scopeFactory) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
            BillingDbContext dbContext = scope.ServiceProvider.GetRequiredService<BillingDbContext>();
            bool canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);

            return canConnect
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy("Não foi possível conectar ao banco de Faturamento.");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy("Não foi possível conectar ao banco de Faturamento.", exception);
        }
    }
}
