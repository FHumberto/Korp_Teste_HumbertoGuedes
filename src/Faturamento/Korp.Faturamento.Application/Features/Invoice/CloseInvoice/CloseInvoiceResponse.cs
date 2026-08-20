namespace Korp.Faturamento.Application.Features.Invoice.CloseInvoice;

public sealed record CloseInvoiceResponse(Guid Id, long Number, string Status, DateTimeOffset ClosedAt);
