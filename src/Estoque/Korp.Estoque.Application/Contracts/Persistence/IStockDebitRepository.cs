namespace Korp.Estoque.Application.Contracts.Persistence;

public interface IStockDebitRepository
{
    Task<StockDebitPersistenceResult> DebitAsync(
        StockDebitPersistenceCommand command,
        CancellationToken cancellationToken);
}

public sealed record StockDebitPersistenceCommand(
    Guid OperationId,
    string IdempotencyKey,
    Guid InvoiceId,
    string PayloadHash,
    DateTimeOffset ProcessedAt,
    IReadOnlyCollection<StockDebitPersistenceItem> Items);

public sealed record StockDebitPersistenceItem(Guid ProductId, int Quantity);

public sealed record StockDebitPersistenceResult(
    StockDebitPersistenceStatus Status,
    Guid OperationId,
    Guid InvoiceId,
    DateTimeOffset ProcessedAt);

public enum StockDebitPersistenceStatus
{
    Succeeded,
    AlreadyProcessed,
    ProductNotFound,
    InsufficientStock,
    IdempotencyConflict
}
