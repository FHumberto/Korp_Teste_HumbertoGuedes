using Korp.Estoque.Domain.Abstractions.Exceptions;
using Korp.Estoque.Domain.Abstractions.Types;
using Korp.Estoque.Domain.Entities.Errors;

namespace Korp.Estoque.Domain.Entities;

public sealed class Product : Entity
{
    #region [ CONSTANTES ]

    public const int MaxCodeLength = 50;
    public const int MaxDescriptionLength = 200;

    #endregion

    #region [ PROPRIEDADES ]

    public string Code { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public int Balance { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    #endregion

    #region [ CONSTRUTORES ]

    private Product() { }

    private Product(Guid id, string code, string description, int initialBalance, DateTimeOffset createdAt) : base(id)
    {
        Code = code;
        Description = description;
        Balance = initialBalance;
        CreatedAt = createdAt;

        Validate();
    }

    #endregion

    #region [ CRIAÇÃO ]

    public static Product Create(Guid id, string code, string description, int initialBalance, DateTimeOffset createdAt) => new(id, code, description, initialBalance, createdAt);

    #endregion

    #region [ COMPORTAMENTOS ]

    public void Debit(int quantity, DateTimeOffset updatedAt)
    {
        if (quantity <= 0)
            throw new DomainException(ProductErrors.InvalidDebitQuantity);

        if (quantity > Balance)
            throw new DomainException(ProductErrors.InsufficientStock);

        Balance -= quantity;
        UpdatedAt = updatedAt;
    }

    #endregion

    #region [ VALIDAÇÃO ]

    private void Validate()
    {
        if (Id == Guid.Empty)
            throw new DomainException(ProductErrors.IdRequired);

        if (string.IsNullOrWhiteSpace(Code))
            throw new DomainException(ProductErrors.CodeRequired);

        if (Code.Length > MaxCodeLength)
            throw new DomainException(ProductErrors.CodeTooLong);

        if (string.IsNullOrWhiteSpace(Description))
            throw new DomainException(ProductErrors.DescriptionRequired);

        if (Description.Length > MaxDescriptionLength)
            throw new DomainException(ProductErrors.DescriptionTooLong);

        if (Balance < 0)
            throw new DomainException(ProductErrors.NegativeBalance);
    }

    #endregion
}
