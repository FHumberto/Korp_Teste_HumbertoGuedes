using Korp.Estoque.Domain.Abstractions.Types;

namespace Korp.Estoque.Domain.Entities.Errors;

public static class ProductErrors
{
    #region [ ERROS ]

    public static Error IdRequired { get; }
        = Error.Validation("PRODUCT_ID_REQUIRED", "O identificador do produto é obrigatório.");

    public static Error CodeRequired { get; }
        = Error.Validation("PRODUCT_CODE_REQUIRED", "O código do produto é obrigatório.");

    public static Error CodeTooLong { get; }
        = Error.Validation("PRODUCT_CODE_TOO_LONG", $"O código do produto deve possuir no máximo {Product.MaxCodeLength} caracteres.");

    public static Error DescriptionRequired { get; }
        = Error.Validation("PRODUCT_DESCRIPTION_REQUIRED", "A descrição do produto é obrigatória.");

    public static Error DescriptionTooLong { get; }
        = Error.Validation("PRODUCT_DESCRIPTION_TOO_LONG", $"A descrição do produto deve possuir no máximo {Product.MaxDescriptionLength} caracteres.");

    public static Error CodeAlreadyExists { get; }
        = Error.Conflict("PRODUCT_CODE_ALREADY_EXISTS", "Já existe um produto cadastrado com o código informado.");

    public static Error NotFound { get; }
        = Error.NotFound("PRODUCT_NOT_FOUND", "O produto informado não foi encontrado.");

    public static Error NegativeBalance { get; }
        = Error.Validation("PRODUCT_NEGATIVE_BALANCE", "O saldo do produto não pode ser negativo.");

    public static Error InvalidDebitQuantity { get; }
        = Error.Validation("PRODUCT_DEBIT_QUANTITY_INVALID", "A quantidade da baixa deve ser maior que zero.");

    public static Error InsufficientStock { get; }
        = Error.Conflict("INSUFFICIENT_STOCK", "O produto não possui saldo suficiente.");

    #endregion
}
