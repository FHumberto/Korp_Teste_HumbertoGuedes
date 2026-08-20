using Korp.Estoque.Application.Contracts.Persistence;
using Korp.Estoque.Domain.Entities;
using Korp.Estoque.Infrastructure.Persistence.Repositories;
using Korp.Estoque.IntegrationTests.Infrastructure;

namespace Korp.Estoque.IntegrationTests.Persistence;

[Collection(EstoqueIntegrationCollection.Name)]
public sealed class InventoryPersistenceTests(EstoqueDatabaseFixture fixture) : IAsyncLifetime
{
    public Task InitializeAsync() => fixture.ResetAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task ListAvailable_WhenProductsHaveDifferentBalances_ShouldReturnOnlyPositiveBalances()
    {
        Product availableProduct = CreateProduct("PROD-001", 5);
        Product unavailableProduct = CreateProduct("PROD-002", 0);
        await SeedProductsAsync(availableProduct, unavailableProduct);
        await using InventoryDbContext dbContext = fixture.CreateDbContext();
        var repository = new ProductRepository(dbContext);

        IReadOnlyList<Product> products = await repository.ListAvailableAsync(CancellationToken.None);

        products.ShouldHaveSingleItem().Id.ShouldBe(availableProduct.Id);
    }

    // Atende à unicidade do código do produto exigida pelo desafio e garantida pelo índice do SQL Server.
    [Fact]
    public async Task Add_WhenProductCodeAlreadyExists_ShouldRejectDuplicate()
    {
        await using InventoryDbContext firstDbContext = fixture.CreateDbContext();
        var firstRepository = new ProductRepository(firstDbContext);
        bool firstResult = await firstRepository.TryAddAsync(CreateProduct("PROD-001", 10), CancellationToken.None);

        await using InventoryDbContext secondDbContext = fixture.CreateDbContext();
        var secondRepository = new ProductRepository(secondDbContext);
        bool duplicateResult = await secondRepository.TryAddAsync(CreateProduct("PROD-001", 20), CancellationToken.None);

        firstResult.ShouldBeTrue();
        duplicateResult.ShouldBeFalse();
        (await secondDbContext.Products.CountAsync()).ShouldBe(1);
    }

    // Atende à baixa atômica de vários itens: quando todos possuem saldo, todos são debitados na mesma operação.
    [Fact]
    public async Task Debit_WhenAllItemsHaveBalance_ShouldDebitEveryProduct()
    {
        Product firstProduct = CreateProduct("PROD-001", 5);
        Product secondProduct = CreateProduct("PROD-002", 8);
        await SeedProductsAsync(firstProduct, secondProduct);

        StockDebitPersistenceResult result = await DebitAsync(
            CreateCommand("invoice:success:close:v1", "hash-success", (firstProduct.Id, 2), (secondProduct.Id, 3)));

        result.Status.ShouldBe(StockDebitPersistenceStatus.Succeeded);
        (await GetBalanceAsync(firstProduct.Id)).ShouldBe(3);
        (await GetBalanceAsync(secondProduct.Id)).ShouldBe(5);
    }

    // Atende ao rollback integral: saldo insuficiente em um item não pode produzir baixa parcial nos demais.
    [Fact]
    public async Task Debit_WhenOneItemHasInsufficientStock_ShouldRollbackEveryDebit()
    {
        Product availableProduct = CreateProduct("PROD-001", 5);
        Product unavailableProduct = CreateProduct("PROD-002", 0);
        await SeedProductsAsync(availableProduct, unavailableProduct);

        StockDebitPersistenceResult result = await DebitAsync(
            CreateCommand("invoice:rollback:close:v1", "hash-rollback", (availableProduct.Id, 2), (unavailableProduct.Id, 1)));

        result.Status.ShouldBe(StockDebitPersistenceStatus.InsufficientStock);
        (await GetBalanceAsync(availableProduct.Id)).ShouldBe(5);
        (await GetBalanceAsync(unavailableProduct.Id)).ShouldBe(0);
        (await CountOperationsAsync()).ShouldBe(0);
    }

    // Atende à idempotência: repetir a mesma chave e o mesmo payload retorna o resultado anterior sem nova baixa.
    [Fact]
    public async Task Debit_WhenSameIdempotencyKeyAndPayloadAreRepeated_ShouldNotDebitAgain()
    {
        Product product = CreateProduct("PROD-001", 2);
        await SeedProductsAsync(product);
        StockDebitPersistenceCommand command = CreateCommand("invoice:retry:close:v1", "hash-retry", (product.Id, 1));

        StockDebitPersistenceResult firstResult = await DebitAsync(command);
        StockDebitPersistenceResult repeatedResult = await DebitAsync(command with { OperationId = Guid.NewGuid() });

        firstResult.Status.ShouldBe(StockDebitPersistenceStatus.Succeeded);
        repeatedResult.Status.ShouldBe(StockDebitPersistenceStatus.AlreadyProcessed);
        repeatedResult.OperationId.ShouldBe(firstResult.OperationId);
        (await GetBalanceAsync(product.Id)).ShouldBe(1);
        (await CountOperationsAsync()).ShouldBe(1);
    }

    // Atende ao conflito idempotente: a mesma chave não pode ser reutilizada para um conteúdo diferente.
    [Fact]
    public async Task Debit_WhenIdempotencyKeyIsReusedWithDifferentPayload_ShouldReturnConflict()
    {
        Product product = CreateProduct("PROD-001", 3);
        await SeedProductsAsync(product);
        StockDebitPersistenceCommand firstCommand = CreateCommand("invoice:conflict:close:v1", "hash-original", (product.Id, 1));

        StockDebitPersistenceResult firstResult = await DebitAsync(firstCommand);
        StockDebitPersistenceResult conflictingResult = await DebitAsync(
            firstCommand with { OperationId = Guid.NewGuid(), PayloadHash = "hash-different", Items = [new(product.Id, 2)] });

        firstResult.Status.ShouldBe(StockDebitPersistenceStatus.Succeeded);
        conflictingResult.Status.ShouldBe(StockDebitPersistenceStatus.IdempotencyConflict);
        (await GetBalanceAsync(product.Id)).ShouldBe(2);
        (await CountOperationsAsync()).ShouldBe(1);
    }

    // Atende à concorrência na última unidade: apenas uma baixa vence e o saldo nunca fica negativo.
    [Fact]
    public async Task Debit_WhenTwoOperationsCompeteForLastUnit_ShouldAllowOnlyOneDebit()
    {
        Product product = CreateProduct("PROD-001", 1);
        await SeedProductsAsync(product);

        StockDebitPersistenceResult[] results = await Task.WhenAll(
            DebitAsync(CreateCommand("invoice:first:close:v1", "hash-first", (product.Id, 1))),
            DebitAsync(CreateCommand("invoice:second:close:v1", "hash-second", (product.Id, 1))));

        results.Count(result => result.Status == StockDebitPersistenceStatus.Succeeded).ShouldBe(1);
        results.Count(result => result.Status == StockDebitPersistenceStatus.InsufficientStock).ShouldBe(1);
        (await GetBalanceAsync(product.Id)).ShouldBe(0);
        (await CountOperationsAsync()).ShouldBe(1);
    }

    private static Product CreateProduct(string code, int balance)
        => Product.Create(Guid.NewGuid(), code, $"Produto {code}", balance, DateTimeOffset.UtcNow);

    private static StockDebitPersistenceCommand CreateCommand(
        string idempotencyKey,
        string payloadHash,
        params (Guid ProductId, int Quantity)[] items) =>
        new(
            Guid.NewGuid(),
            idempotencyKey,
            Guid.NewGuid(),
            payloadHash,
            DateTimeOffset.UtcNow,
            items.Select(item => new StockDebitPersistenceItem(item.ProductId, item.Quantity)).ToArray());

    private async Task SeedProductsAsync(params Product[] products)
    {
        await using InventoryDbContext dbContext = fixture.CreateDbContext();
        dbContext.Products.AddRange(products);
        await dbContext.SaveChangesAsync();
    }

    private async Task<StockDebitPersistenceResult> DebitAsync(StockDebitPersistenceCommand command)
    {
        await using InventoryDbContext dbContext = fixture.CreateDbContext();
        return await new StockDebitRepository(dbContext).DebitAsync(command, CancellationToken.None);
    }

    private async Task<int> GetBalanceAsync(Guid productId)
    {
        await using InventoryDbContext dbContext = fixture.CreateDbContext();
        return await dbContext.Products.Where(product => product.Id == productId)
            .Select(product => product.Balance)
            .SingleAsync();
    }

    private async Task<int> CountOperationsAsync()
    {
        await using InventoryDbContext dbContext = fixture.CreateDbContext();
        return await dbContext.StockOperations.CountAsync();
    }
}
