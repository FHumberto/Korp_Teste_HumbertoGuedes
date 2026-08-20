namespace Korp.Faturamento.Application.Features.Invoice.CreateInvoice;

public sealed class CreateInvoiceRequest
{
    public IReadOnlyCollection<CreateInvoiceItemRequest>? Items { get; init; } = [];
}

public sealed record CreateInvoiceItemRequest(Guid ProductId, int Quantity);
