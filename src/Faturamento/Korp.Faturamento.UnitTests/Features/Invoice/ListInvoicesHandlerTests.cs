using Korp.Faturamento.Application.Abstractions.Wrappers;
using Korp.Faturamento.Application.Contracts.Persistence;
using Korp.Faturamento.Application.Features.Invoice.ListInvoices;
using Korp.Faturamento.Domain.Enums;
using Shouldly;
using InvoiceEntity = Korp.Faturamento.Domain.Entities.Invoice;

namespace Korp.Faturamento.UnitTests.Features.Invoice;

public sealed class ListInvoicesHandlerTests
{
    [Fact]
    public async Task Execute_WhenStatusIsValid_ShouldFilterAndMapInvoices()
    {
        InvoiceEntity invoice = InvoiceEntity.Create(Guid.NewGuid(), 8, DateTimeOffset.UtcNow);
        invoice.AddItem(Guid.NewGuid(), "PROD-001", "Produto", 1);
        var repository = new FakeInvoiceRepository([invoice]);
        var handler = new ListInvoicesHandler(new ListInvoicesValidator(), repository);

        Result<IReadOnlyCollection<ListInvoicesResponse>> result = await handler.ExecuteAsync(new ListInvoicesRequest { Status = "open" }, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldHaveSingleItem().ItemCount.ShouldBe(1);
        repository.RequestedStatus.ShouldBe(InvoiceStatus.Open);
    }

    [Fact]
    public async Task Execute_WhenStatusIsInvalid_ShouldReturnValidationError()
    {
        var repository = new FakeInvoiceRepository([]);
        var handler = new ListInvoicesHandler(new ListInvoicesValidator(), repository);

        Result<IReadOnlyCollection<ListInvoicesResponse>> result = await handler.ExecuteAsync(new ListInvoicesRequest { Status = "cancelled" }, CancellationToken.None);

        result.IsSuccess.ShouldBeFalse();
        result.Error!.Code.ShouldBe("VALIDATION_ERROR");
        repository.WasCalled.ShouldBeFalse();
    }

    private sealed class FakeInvoiceRepository(IReadOnlyCollection<InvoiceEntity> invoices) : IInvoiceRepository
    {
        public bool WasCalled { get; private set; }
        public InvoiceStatus? RequestedStatus { get; private set; }
        public Task<InvoiceEntity?> GetByIdAsync(Guid invoiceId, CancellationToken cancellationToken) => Task.FromResult<InvoiceEntity?>(null);
        public Task<IReadOnlyCollection<InvoiceEntity>> ListAsync(InvoiceStatus? status, CancellationToken cancellationToken)
        {
            WasCalled = true;
            RequestedStatus = status;
            return Task.FromResult(invoices);
        }
        public Task AddAsync(InvoiceEntity invoice, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
