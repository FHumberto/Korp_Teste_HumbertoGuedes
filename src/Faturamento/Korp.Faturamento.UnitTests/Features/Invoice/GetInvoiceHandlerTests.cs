using Korp.Faturamento.Application.Abstractions.Wrappers;
using Korp.Faturamento.Application.Contracts.Persistence;
using Korp.Faturamento.Application.Features.Invoice.GetInvoice;
using Korp.Faturamento.Domain.Enums;
using InvoiceEntity = Korp.Faturamento.Domain.Entities.Invoice;

namespace Korp.Faturamento.UnitTests.Features.Invoice;

public sealed class GetInvoiceHandlerTests
{
    [Fact]
    public async Task Execute_WhenInvoiceExists_ShouldReturnInvoiceWithItems()
    {
        InvoiceEntity invoice = CreateInvoice(7);
        var handler = new GetInvoiceHandler(new FakeInvoiceRepository(invoice));

        Result<GetInvoiceResponse> result = await handler.ExecuteAsync(invoice.Id, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Number.ShouldBe(7);
        result.Value.Status.ShouldBe("open");
        result.Value.Items.ShouldHaveSingleItem().ProductCode.ShouldBe("PROD-001");
    }

    [Fact]
    public async Task Execute_WhenInvoiceDoesNotExist_ShouldReturnNotFound()
    {
        var handler = new GetInvoiceHandler(new FakeInvoiceRepository(null));

        Result<GetInvoiceResponse> result = await handler.ExecuteAsync(Guid.NewGuid(), CancellationToken.None);

        result.IsSuccess.ShouldBeFalse();
        result.Error!.Code.ShouldBe("INVOICE_NOT_FOUND");
    }

    private static InvoiceEntity CreateInvoice(long number)
    {
        InvoiceEntity invoice = InvoiceEntity.Create(Guid.NewGuid(), number, DateTimeOffset.UtcNow);
        invoice.AddItem(Guid.NewGuid(), "PROD-001", "Produto", 2);
        return invoice;
    }

    private sealed class FakeInvoiceRepository(InvoiceEntity? invoice) : IInvoiceRepository
    {
        public Task<InvoiceEntity?> GetByIdAsync(Guid invoiceId, CancellationToken cancellationToken) => Task.FromResult(invoice);
        public Task<IReadOnlyCollection<InvoiceEntity>> ListAsync(InvoiceStatus? status, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyCollection<InvoiceEntity>>([]);
        public Task AddAsync(InvoiceEntity value, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
