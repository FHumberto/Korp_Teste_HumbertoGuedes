using Korp.Faturamento.Domain.Abstractions.Types;

namespace Korp.Faturamento.Application.Features.Invoice.CreateInvoice;

public static class CreateInvoiceErrors
{
    public static Error ProductNotFound => Error.NotFound("PRODUCT_NOT_FOUND", "Um ou mais produtos informados não foram encontrados.");
    public static Error InventoryUnavailable => Error.ServiceUnavailable("INVENTORY_UNAVAILABLE", "O serviço de Estoque está indisponível.");
}
