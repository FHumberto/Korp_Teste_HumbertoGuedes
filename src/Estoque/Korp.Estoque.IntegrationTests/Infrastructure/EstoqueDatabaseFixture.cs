using Microsoft.Data.SqlClient;
using Testcontainers.MsSql;

namespace Korp.Estoque.IntegrationTests.Infrastructure;

public sealed class EstoqueDatabaseFixture : IAsyncLifetime
{
    private const string DatabaseName = "KorpEstoqueIntegrationTests";

    private readonly MsSqlContainer _sqlServer = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest")
        .Build();

    public string ConnectionString { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        await _sqlServer.StartAsync();

        string masterConnectionString = _sqlServer.GetConnectionString();
        await using var connection = new SqlConnection(masterConnectionString);
        await connection.OpenAsync();

        await using (SqlCommand command = connection.CreateCommand())
        {
            command.CommandText = $"CREATE DATABASE [{DatabaseName}]";
            await command.ExecuteNonQueryAsync();
        }

        var connectionStringBuilder = new SqlConnectionStringBuilder(masterConnectionString)
        {
            InitialCatalog = DatabaseName
        };
        ConnectionString = connectionStringBuilder.ConnectionString;

        await using InventoryDbContext dbContext = CreateDbContext();
        await dbContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync() => await _sqlServer.DisposeAsync();

    public InventoryDbContext CreateDbContext()
    {
        DbContextOptions<InventoryDbContext> options = new DbContextOptionsBuilder<InventoryDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;
        return new InventoryDbContext(options);
    }

    public async Task ResetAsync()
    {
        await using InventoryDbContext dbContext = CreateDbContext();
        await dbContext.StockOperations.ExecuteDeleteAsync();
        await dbContext.Products.ExecuteDeleteAsync();
    }
}

[CollectionDefinition(EstoqueIntegrationCollection.Name, DisableParallelization = true)]
public sealed class EstoqueIntegrationCollection : ICollectionFixture<EstoqueDatabaseFixture>
{
    public const string Name = "Estoque SQL Server integration";
}
