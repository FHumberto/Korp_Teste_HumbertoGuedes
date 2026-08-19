using Korp.Faturamento.Domain.Abstractions.Types;

namespace Korp.Faturamento.Domain.Entities.Errors;

public static class InvoiceErrors
{
    public static Error IdRequired { get; }
        = Error.Validation("INVOICE_ID_REQUIRED", "O identificador da nota é obrigatório.");

    public static Error InvalidNumber { get; }
        = Error.Validation("INVOICE_NUMBER_INVALID", "O número da nota deve ser maior que zero.");

    public static Error ClosedModification { get; }
        = Error.Conflict("INVOICE_CLOSED_MODIFICATION", "Não é possível alterar uma nota fechada.");

    public static Error DuplicateProduct { get; }
        = Error.Validation("INVOICE_DUPLICATE_PRODUCT", "O produto já foi incluído na nota.");

    public static Error AlreadyClosed { get; }
        = Error.Conflict("INVOICE_ALREADY_CLOSED", "A nota já está fechada.");

    public static Error WithoutItems { get; }
        = Error.Conflict("INVOICE_WITHOUT_ITEMS", "A nota deve possuir ao menos um item para ser fechada.");
}
