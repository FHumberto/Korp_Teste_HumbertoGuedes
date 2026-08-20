namespace Korp.Faturamento.Application.Features.Invoice.GetInvoiceDocument;

public static class GetInvoiceDocumentErrors
{
    public static Error GenerationFailed => Error.Failure("PDF_GENERATION_ERROR", "Não foi possível gerar o documento da nota.");
}
