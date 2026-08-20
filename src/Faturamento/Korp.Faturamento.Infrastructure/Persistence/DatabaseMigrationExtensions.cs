using Microsoft.Extensions.DependencyInjection;

namespace Korp.Faturamento.Infrastructure.Persistence;

public static class DatabaseMigrationExtensions
{
    public static async Task ApplyBillingMigrationsAsync(this IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        await using AsyncServiceScope scope = serviceProvider.CreateAsyncScope();
        BillingDbContext dbContext = scope.ServiceProvider.GetRequiredService<BillingDbContext>();

        IEnumerable<string> pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync(cancellationToken);
        if (!pendingMigrations.Any())
            return;

        await dbContext.Database.MigrateAsync(cancellationToken);
    }
}
