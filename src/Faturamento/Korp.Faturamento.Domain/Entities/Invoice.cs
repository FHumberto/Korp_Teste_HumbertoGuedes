using Korp.Faturamento.Domain.Abstractions.Exceptions;
using Korp.Faturamento.Domain.Abstractions.Types;
using Korp.Faturamento.Domain.Entities.Errors;
using Korp.Faturamento.Domain.Enums;

namespace Korp.Faturamento.Domain.Entities;

public sealed class Invoice : Entity
{
    #region [ CAMPOS ]

    private readonly List<InvoiceItem> _items = [];

    #endregion

    #region [ PROPRIEDADES ]

    public long Number { get; private set; }
    public InvoiceStatus Status { get; private set; }
    public IReadOnlyCollection<InvoiceItem> Items => _items.AsReadOnly();
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? ClosedAt { get; private set; }

    #endregion

    #region [ CONSTRUTORES ]

    private Invoice() { }

    private Invoice(Guid id, long number, DateTimeOffset createdAt) : base(id)
    {
        if (id == Guid.Empty)
            throw new DomainException(InvoiceErrors.IdRequired);

        if (number <= 0)
            throw new DomainException(InvoiceErrors.InvalidNumber);

        Number = number;
        Status = InvoiceStatus.Open;
        CreatedAt = createdAt;
    }

    #endregion

    #region [ CRIAÇÃO ]

    public static Invoice Create(Guid id, long number, DateTimeOffset createdAt) => new(id, number, createdAt);

    #endregion

    #region [ COMPORTAMENTOS ]

    public void AddItem(Guid productId, string productCode, string productDescription, int quantity)
    {
        if (Status != InvoiceStatus.Open)
            throw new DomainException(InvoiceErrors.ClosedModification);

        if (_items.Any(item => item.ProductId == productId))
            throw new DomainException(InvoiceErrors.DuplicateProduct);

        var item = InvoiceItem.Create(productId, productCode, productDescription, quantity);

        _items.Add(item);
    }

    public void Close(DateTimeOffset closedAt)
    {
        if (Status != InvoiceStatus.Open)
            throw new DomainException(InvoiceErrors.AlreadyClosed);

        if (_items.Count == 0)
            throw new DomainException(InvoiceErrors.WithoutItems);

        Status = InvoiceStatus.Closed;
        ClosedAt = closedAt;
    }

    #endregion
}
