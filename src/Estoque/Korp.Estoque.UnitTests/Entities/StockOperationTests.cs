using Korp.Estoque.Domain.Abstractions.Exceptions;
using Korp.Estoque.Domain.Entities;
using Korp.Estoque.Domain.Entities.Errors;
using Shouldly;

namespace Korp.Estoque.UnitTests.Entities;

public sealed class StockOperationTests
{
    private static readonly DateTimeOffset ProcessedAt = new(2026, 8, 19, 12, 5, 0, TimeSpan.Zero);

    [Fact]
    public void Create_WithValidData_ShouldSetIdempotencyState()
    {
        Guid operationId = Guid.NewGuid();
        Guid invoiceId = Guid.NewGuid();
        string idempotencyKey = $"invoice:{invoiceId}:close:v1";

        StockOperation operation = StockOperation.Create(
            operationId,
            idempotencyKey,
            invoiceId,
            "payload-hash",
            ProcessedAt);

        operation.Id.ShouldBe(operationId);
        operation.IdempotencyKey.ShouldBe(idempotencyKey);
        operation.InvoiceId.ShouldBe(invoiceId);
        operation.PayloadHash.ShouldBe("payload-hash");
        operation.ProcessedAt.ShouldBe(ProcessedAt);
    }

    [Fact]
    public void Create_WithoutIdempotencyKey_ShouldFail()
    {
        Action action = () => StockOperation.Create(
            Guid.NewGuid(),
            string.Empty,
            Guid.NewGuid(),
            "payload-hash",
            ProcessedAt);

        DomainException exception = action.ShouldThrow<DomainException>();

        exception.Error.ShouldBe(StockOperationErrors.IdempotencyKeyRequired);
    }
}
