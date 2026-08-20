namespace Korp.Faturamento.Application.Contracts.Gateways;

public interface IInventoryGateway
{
    Task<IReadOnlyCollection<InventoryProduct>> GetProductsByIdsAsync(
        IReadOnlyCollection<Guid> productIds,
        CancellationToken cancellationToken);

    Task<DebitStockResult> DebitAsync(
        DebitStockCommand command,
        string idempotencyKey,
        CancellationToken cancellationToken);
}

public sealed record InventoryProduct(Guid Id, string Code, string Description, int Balance);

public sealed record DebitStockCommand(Guid InvoiceId, IReadOnlyCollection<DebitStockItem> Items);

public sealed record DebitStockItem(Guid ProductId, int Quantity);

public sealed record DebitStockResult(DebitStockStatus Status)
{
    public static DebitStockResult Succeeded { get; } = new(DebitStockStatus.Succeeded);
}

public enum DebitStockStatus
{
    Succeeded,
    ProductNotFound,
    InsufficientStock,
    IdempotencyConflict
}
