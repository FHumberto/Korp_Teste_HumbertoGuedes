namespace Korp.Estoque.Application.Features.Stock.DebitStock;

public sealed record DebitStockCommand(string? IdempotencyKey, Guid InvoiceId, IReadOnlyCollection<DebitStockItemRequest> Items);
