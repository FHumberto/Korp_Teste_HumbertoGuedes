using Korp.Faturamento.Domain.Abstractions.Exceptions;
using Korp.Faturamento.Domain.Abstractions.Types;
using Korp.Faturamento.Domain.Entities.Errors;

namespace Korp.Faturamento.Domain.Entities;

public sealed class InvoiceItem : Entity
{
    #region [ CONSTANTES ]

    public const int MaxProductCodeLength = 50;
    public const int MaxProductDescriptionLength = 200;

    #endregion

    #region [ PROPRIEDADES ]

    public Guid ProductId { get; private set; }
    public string ProductCode { get; private set; } = string.Empty;
    public string ProductDescription { get; private set; } = string.Empty;
    public int Quantity { get; private set; }

    #endregion

    #region [ CONSTRUTORES ]

    private InvoiceItem()
    {
    }

    private InvoiceItem(
        Guid id,
        Guid productId,
        string productCode,
        string productDescription,
        int quantity)
        : base(id)
    {
        if (productId == Guid.Empty)
        {
            throw new DomainException(InvoiceItemErrors.ProductIdRequired);
        }

        if (string.IsNullOrWhiteSpace(productCode))
        {
            throw new DomainException(InvoiceItemErrors.ProductCodeRequired);
        }

        if (productCode.Length > MaxProductCodeLength)
        {
            throw new DomainException(InvoiceItemErrors.ProductCodeTooLong);
        }

        if (string.IsNullOrWhiteSpace(productDescription))
        {
            throw new DomainException(InvoiceItemErrors.ProductDescriptionRequired);
        }

        if (productDescription.Length > MaxProductDescriptionLength)
        {
            throw new DomainException(InvoiceItemErrors.ProductDescriptionTooLong);
        }

        if (quantity <= 0)
        {
            throw new DomainException(InvoiceItemErrors.InvalidQuantity);
        }

        ProductId = productId;
        ProductCode = productCode;
        ProductDescription = productDescription;
        Quantity = quantity;
    }

    #endregion

    #region [ CRIAÇÃO ]

    internal static InvoiceItem Create(
        Guid productId,
        string productCode,
        string productDescription,
        int quantity) => new(
            Guid.NewGuid(),
            productId,
            productCode,
            productDescription,
            quantity);

    #endregion
}
