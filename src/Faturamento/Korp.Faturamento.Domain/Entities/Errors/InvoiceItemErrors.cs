using Korp.Faturamento.Domain.Abstractions.Types;

namespace Korp.Faturamento.Domain.Entities.Errors;

public static class InvoiceItemErrors
{
    public static Error ProductIdRequired { get; }
        = Error.Validation("INVOICE_ITEM_PRODUCT_ID_REQUIRED", "O identificador do produto é obrigatório.");

    public static Error ProductCodeRequired { get; }
        = Error.Validation("INVOICE_ITEM_PRODUCT_CODE_REQUIRED", "O código do produto é obrigatório.");

    public static Error ProductCodeTooLong { get; }
        = Error.Validation("INVOICE_ITEM_PRODUCT_CODE_TOO_LONG", $"O código do produto deve possuir no máximo {InvoiceItem.MaxProductCodeLength} caracteres.");

    public static Error ProductDescriptionRequired { get; }
        = Error.Validation("INVOICE_ITEM_PRODUCT_DESCRIPTION_REQUIRED", "A descrição do produto é obrigatória.");

    public static Error ProductDescriptionTooLong { get; }
        = Error.Validation("INVOICE_ITEM_PRODUCT_DESCRIPTION_TOO_LONG", $"A descrição do produto deve possuir no máximo {InvoiceItem.MaxProductDescriptionLength} caracteres.");

    public static Error InvalidQuantity { get; }
        = Error.Validation("INVOICE_ITEM_QUANTITY_INVALID", "A quantidade do item deve ser maior que zero.");
}
