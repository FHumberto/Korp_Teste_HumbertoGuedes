namespace Korp.Faturamento.Application.Features.Invoice.ListInvoices;

public sealed record ListInvoicesResponse(
    Guid Id,
    long Number,
    string Status,
    int ItemCount,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ClosedAt);
