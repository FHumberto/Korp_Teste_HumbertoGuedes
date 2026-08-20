namespace Korp.Estoque.Domain.Entities.Errors;

public static class StockOperationErrors
{
    public static Error IdRequired { get; }
        = Error.Validation("STOCK_OPERATION_ID_REQUIRED", "O identificador da operação de estoque é obrigatório.");

    public static Error IdempotencyKeyRequired { get; }
        = Error.Validation("IDEMPOTENCY_KEY_REQUIRED", "A chave de idempotência é obrigatória.");

    public static Error IdempotencyKeyTooLong { get; }
        = Error.Validation("IDEMPOTENCY_KEY_TOO_LONG", $"A chave de idempotência deve possuir no máximo {StockOperation.MaxIdempotencyKeyLength} caracteres.");

    public static Error InvoiceIdRequired { get; }
        = Error.Validation("INVOICE_ID_REQUIRED", "O identificador da nota é obrigatório.");

    public static Error PayloadHashRequired { get; }
        = Error.Validation("PAYLOAD_HASH_REQUIRED", "O hash do conteúdo da operação é obrigatório.");

    public static Error PayloadHashTooLong { get; }
        = Error.Validation("PAYLOAD_HASH_TOO_LONG", $"O hash do conteúdo deve possuir no máximo {StockOperation.MaxPayloadHashLength} caracteres.");

    public static Error IdempotencyConflict { get; }
        = Error.Conflict("IDEMPOTENCY_CONFLICT", "A chave de idempotência já foi utilizada com outro conteúdo.");
}
