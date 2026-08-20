namespace Korp.Faturamento.Application.Features.Invoice.CreateInvoice;

public sealed record CreateInvoiceResponse(
    Guid Id,
    long Number,
    string Status,
    IReadOnlyCollection<CreateInvoiceItemResponse> Items,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ClosedAt);

public sealed record CreateInvoiceItemResponse(
    Guid ProductId,
    string ProductCode,
    string ProductDescription,
    int Quantity);
