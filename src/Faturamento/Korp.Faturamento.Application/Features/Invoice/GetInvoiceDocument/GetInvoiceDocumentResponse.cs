namespace Korp.Faturamento.Application.Features.Invoice.GetInvoiceDocument;

public sealed record GetInvoiceDocumentResponse(byte[] Content, string FileName);
