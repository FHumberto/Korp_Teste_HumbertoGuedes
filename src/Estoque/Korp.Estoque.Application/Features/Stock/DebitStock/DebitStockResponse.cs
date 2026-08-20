namespace Korp.Estoque.Application.Features.Stock.DebitStock;

public sealed record DebitStockResponse(
    Guid OperationId,
    Guid InvoiceId,
    DateTimeOffset ProcessedAt,
    bool AlreadyProcessed);
