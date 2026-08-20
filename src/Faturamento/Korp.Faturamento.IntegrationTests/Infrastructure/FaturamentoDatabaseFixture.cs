using Korp.Estoque.Infrastructure.Persistence;
using Korp.Faturamento.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;

namespace Korp.Faturamento.IntegrationTests.Infrastructure;

public sealed class FaturamentoDatabaseFixture : IAsyncLifetime
{
    private const string BillingDatabaseName = "KorpFaturamentoTests";
    private const string InventoryDatabaseName = "KorpEstoqueTests";

    private readonly MsSqlContainer _sqlServer = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest")
        .Build();

    public string BillingConnectionString { get; private set; } = string.Empty;
    public string InventoryConnectionString { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        await _sqlServer.StartAsync();

        string masterConnectionString = _sqlServer.GetConnectionString();
        await using var connection = new SqlConnection(masterConnectionString);
        await connection.OpenAsync();

        await using (SqlCommand command = connection.CreateCommand())
        {
            command.CommandText = $"CREATE DATABASE [{BillingDatabaseName}]; CREATE DATABASE [{InventoryDatabaseName}]";
            await command.ExecuteNonQueryAsync();
        }

        BillingConnectionString = CreateDatabaseConnectionString(masterConnectionString, BillingDatabaseName);
        InventoryConnectionString = CreateDatabaseConnectionString(masterConnectionString, InventoryDatabaseName);

        await using BillingDbContext billingDbContext = CreateBillingDbContext();
        await billingDbContext.Database.MigrateAsync();

        await using InventoryDbContext inventoryDbContext = CreateInventoryDbContext();
        await inventoryDbContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync() => await _sqlServer.DisposeAsync();

    public BillingDbContext CreateBillingDbContext()
    {
        DbContextOptions<BillingDbContext> options = new DbContextOptionsBuilder<BillingDbContext>()
            .UseSqlServer(BillingConnectionString)
            .Options;
        return new BillingDbContext(options);
    }

    public InventoryDbContext CreateInventoryDbContext()
    {
        DbContextOptions<InventoryDbContext> options = new DbContextOptionsBuilder<InventoryDbContext>()
            .UseSqlServer(InventoryConnectionString)
            .Options;
        return new InventoryDbContext(options);
    }

    public async Task ResetAsync()
    {
        await using BillingDbContext billingDbContext = CreateBillingDbContext();
        await billingDbContext.InvoiceItems.ExecuteDeleteAsync();
        await billingDbContext.Invoices.ExecuteDeleteAsync();
        await billingDbContext.Database.ExecuteSqlRawAsync("ALTER SEQUENCE [invoice_number_sequence] RESTART WITH 1");

        await using InventoryDbContext inventoryDbContext = CreateInventoryDbContext();
        await inventoryDbContext.StockOperations.ExecuteDeleteAsync();
        await inventoryDbContext.Products.ExecuteDeleteAsync();
    }

    private static string CreateDatabaseConnectionString(string masterConnectionString, string databaseName)
    {
        var builder = new SqlConnectionStringBuilder(masterConnectionString)
        {
            InitialCatalog = databaseName
        };
        return builder.ConnectionString;
    }
}

[CollectionDefinition(FaturamentoIntegrationCollection.Name, DisableParallelization = true)]
public sealed class FaturamentoIntegrationCollection : ICollectionFixture<FaturamentoDatabaseFixture>
{
    public const string Name = "Faturamento SQL Server integration";
}
