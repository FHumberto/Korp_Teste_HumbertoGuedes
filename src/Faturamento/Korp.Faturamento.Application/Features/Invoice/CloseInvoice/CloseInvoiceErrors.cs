namespace Korp.Faturamento.Application.Features.Invoice.CloseInvoice;

public static class CloseInvoiceErrors
{
    public static Error ProductNotFound => Error.NotFound("PRODUCT_NOT_FOUND", "Um produto da nota não foi encontrado no Estoque.");
    public static Error InsufficientStock => Error.Conflict("INSUFFICIENT_STOCK", "Um ou mais produtos não possuem saldo suficiente.");
    public static Error IdempotencyConflict => Error.Conflict("IDEMPOTENCY_CONFLICT", "A operação de fechamento conflita com uma operação já processada.");
    public static Error InventoryUnavailable => Error.ServiceUnavailable("INVENTORY_UNAVAILABLE", "O serviço de Estoque está indisponível.");
}
