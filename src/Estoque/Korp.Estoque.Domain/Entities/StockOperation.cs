using Korp.Estoque.Domain.Abstractions.Exceptions;
using Korp.Estoque.Domain.Abstractions.Types;
using Korp.Estoque.Domain.Entities.Errors;

namespace Korp.Estoque.Domain.Entities;

public sealed class StockOperation : Entity
{
    #region [ CONSTANTES ]

    public const int MaxIdempotencyKeyLength = 200;
    public const int MaxPayloadHashLength = 128;

    #endregion

    #region [ PROPRIEDADES ]

    public string IdempotencyKey { get; private set; } = string.Empty;
    public Guid InvoiceId { get; private set; }
    public string PayloadHash { get; private set; } = string.Empty;
    public DateTimeOffset ProcessedAt { get; private set; }

    #endregion

    #region [ CONSTRUTORES ]

    private StockOperation() { }

    private StockOperation(Guid id, string idempotencyKey, Guid invoiceId, string payloadHash, DateTimeOffset processedAt) : base(id)
    {
        IdempotencyKey = idempotencyKey;
        InvoiceId = invoiceId;
        PayloadHash = payloadHash;
        ProcessedAt = processedAt;

        Validate();
    }

    #endregion

    #region [ MANIPULAÇÃO ]

    public static StockOperation Create(Guid id, string idempotencyKey, Guid invoiceId, string payloadHash, DateTimeOffset processedAt)
    {
        return new(id, idempotencyKey, invoiceId, payloadHash, processedAt);
    }

    #endregion

    #region [ VALIDAÇÃO ]

    private void Validate()
    {
        if (Id == Guid.Empty)
            throw new DomainException(StockOperationErrors.IdRequired);

        if (string.IsNullOrWhiteSpace(IdempotencyKey))
            throw new DomainException(StockOperationErrors.IdempotencyKeyRequired);

        if (IdempotencyKey.Length > MaxIdempotencyKeyLength)
            throw new DomainException(StockOperationErrors.IdempotencyKeyTooLong);

        if (InvoiceId == Guid.Empty)
            throw new DomainException(StockOperationErrors.InvoiceIdRequired);

        if (string.IsNullOrWhiteSpace(PayloadHash))
            throw new DomainException(StockOperationErrors.PayloadHashRequired);

        if (PayloadHash.Length > MaxPayloadHashLength)
            throw new DomainException(StockOperationErrors.PayloadHashTooLong);
    }

    #endregion
}
