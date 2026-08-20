namespace Korp.Faturamento.Application.Features.Invoice.GetInvoice;

public sealed record GetInvoiceResponse(
    Guid Id,
    long Number,
    string Status,
    IReadOnlyCollection<GetInvoiceItemResponse> Items,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ClosedAt);

public sealed record GetInvoiceItemResponse(Guid ProductId, string ProductCode, string ProductDescription, int Quantity);
