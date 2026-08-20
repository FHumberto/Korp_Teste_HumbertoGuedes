using Microsoft.Extensions.DependencyInjection;

namespace Korp.Estoque.Infrastructure.Persistence;

public static class DatabaseMigrationExtensions
{
    public static async Task ApplyInventoryMigrationsAsync(this IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        await using AsyncServiceScope scope = serviceProvider.CreateAsyncScope();
        InventoryDbContext dbContext = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();

        await dbContext.Database.MigrateAsync(cancellationToken);
    }
}
