namespace Korp.Estoque.Application.Features.Stock.DebitStock;

public sealed class DebitStockRequest
{
    public Guid InvoiceId { get; init; }
    public IReadOnlyCollection<DebitStockItemRequest> Items { get; init; } = [];
}

public sealed record DebitStockItemRequest(Guid ProductId, int Quantity);
