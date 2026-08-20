using Korp.Estoque.Application.Abstractions.Wrappers;
using Korp.Estoque.Application.Contracts.Persistence;
using Korp.Estoque.Application.Features.Stock.DebitStock;

namespace Korp.Estoque.UnitTests.Features.Stock;

public sealed class DebitStockHandlerTests
{
    private static readonly DateTimeOffset ProcessedAt = new(2026, 8, 19, 12, 5, 0, TimeSpan.Zero);

    [Theory]
    [InlineData(StockDebitPersistenceStatus.Succeeded, false)]
    [InlineData(StockDebitPersistenceStatus.AlreadyProcessed, true)]
    public async Task Execute_WhenPersistenceSucceeds_ShouldReturnProcessingState(
        StockDebitPersistenceStatus status,
        bool alreadyProcessed)
    {
        Guid operationId = Guid.NewGuid();
        Guid invoiceId = Guid.NewGuid();
        StockDebitRepositoryStub repository = new(status, operationId, invoiceId, ProcessedAt);
        DebitStockHandler handler = CreateHandler(repository);
        DebitStockCommand command = CreateValidCommand(invoiceId);

        Result<DebitStockResponse> result = await handler.ExecuteAsync(command, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.OperationId.ShouldBe(operationId);
        result.Value.InvoiceId.ShouldBe(invoiceId);
        result.Value.ProcessedAt.ShouldBe(ProcessedAt);
        result.Value.AlreadyProcessed.ShouldBe(alreadyProcessed);
    }

    [Theory]
    [InlineData(StockDebitPersistenceStatus.ProductNotFound, "PRODUCT_NOT_FOUND")]
    [InlineData(StockDebitPersistenceStatus.InsufficientStock, "INSUFFICIENT_STOCK")]
    [InlineData(StockDebitPersistenceStatus.IdempotencyConflict, "IDEMPOTENCY_CONFLICT")]
    public async Task Execute_WhenPersistenceRejects_ShouldReturnExpectedError(
        StockDebitPersistenceStatus status,
        string expectedErrorCode)
    {
        Guid invoiceId = Guid.NewGuid();
        StockDebitRepositoryStub repository = new(status, Guid.NewGuid(), invoiceId, ProcessedAt);
        DebitStockHandler handler = CreateHandler(repository);

        Result<DebitStockResponse> result = await handler.ExecuteAsync(
            CreateValidCommand(invoiceId),
            CancellationToken.None);

        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNull();
        result.Error.Code.ShouldBe(expectedErrorCode);
    }

    [Fact]
    public async Task Execute_WithInvalidCommand_ShouldReturnValidationErrorBeforePersistence()
    {
        StockDebitRepositoryStub repository = new(
            StockDebitPersistenceStatus.Succeeded,
            Guid.NewGuid(),
            Guid.NewGuid(),
            ProcessedAt);
        DebitStockHandler handler = CreateHandler(repository);
        DebitStockCommand command = new(null, Guid.Empty, []);

        Result<DebitStockResponse> result = await handler.ExecuteAsync(command, CancellationToken.None);

        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNull();
        result.Error.Code.ShouldBe("VALIDATION_ERROR");
        repository.Commands.ShouldBeEmpty();
    }

    [Fact]
    public async Task Execute_WithItemsInDifferentOrder_ShouldProduceSameCanonicalHash()
    {
        Guid invoiceId = Guid.NewGuid();
        DebitStockItemRequest firstItem = new(Guid.NewGuid(), 1);
        DebitStockItemRequest secondItem = new(Guid.NewGuid(), 2);
        StockDebitRepositoryStub repository = new(
            StockDebitPersistenceStatus.Succeeded,
            Guid.NewGuid(),
            invoiceId,
            ProcessedAt);
        DebitStockHandler handler = CreateHandler(repository);

        await handler.ExecuteAsync(
            CreateValidCommand(invoiceId, [firstItem, secondItem]),
            CancellationToken.None);
        await handler.ExecuteAsync(
            CreateValidCommand(invoiceId, [secondItem, firstItem]),
            CancellationToken.None);

        repository.Commands.Count.ShouldBe(2);
        repository.Commands[0].PayloadHash.ShouldBe(repository.Commands[1].PayloadHash);
        repository.Commands[0].Items.Select(item => item.ProductId)
            .ShouldBe(repository.Commands[1].Items.Select(item => item.ProductId));
    }

    private static DebitStockHandler CreateHandler(IStockDebitRepository repository) =>
        new(new DebitStockCommandValidator(), repository, new FixedTimeProvider(ProcessedAt));

    private static DebitStockCommand CreateValidCommand(
        Guid invoiceId,
        IReadOnlyCollection<DebitStockItemRequest>? items = null) =>
        new(
            $"invoice:{invoiceId}:close:v1",
            invoiceId,
            items ?? [new DebitStockItemRequest(Guid.NewGuid(), 2)]);

    private sealed class StockDebitRepositoryStub(
        StockDebitPersistenceStatus status,
        Guid operationId,
        Guid invoiceId,
        DateTimeOffset processedAt) : IStockDebitRepository
    {
        public List<StockDebitPersistenceCommand> Commands { get; } = [];

        public Task<StockDebitPersistenceResult> DebitAsync(
            StockDebitPersistenceCommand command,
            CancellationToken cancellationToken)
        {
            Commands.Add(command);
            return Task.FromResult(new StockDebitPersistenceResult(
                status,
                operationId,
                invoiceId,
                processedAt));
        }
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
