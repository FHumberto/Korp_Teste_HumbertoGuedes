using Korp.Faturamento.Application.Abstractions.Wrappers;
using Korp.Faturamento.Application.Contracts.Documents;
using Korp.Faturamento.Application.Contracts.Persistence;
using Korp.Faturamento.Application.Features.Invoice.GetInvoiceDocument;
using Korp.Faturamento.Domain.Enums;
using InvoiceEntity = Korp.Faturamento.Domain.Entities.Invoice;

namespace Korp.Faturamento.UnitTests.Features.Invoice;

public sealed class GetInvoiceDocumentHandlerTests
{
    [Fact]
    public async Task Execute_WhenInvoiceIsClosed_ShouldGenerateDocument()
    {
        InvoiceEntity invoice = CreateInvoice();
        invoice.Close(DateTimeOffset.UtcNow);
        var generator = new FakeDocumentGenerator();
        var handler = new GetInvoiceDocumentHandler(new FakeInvoiceRepository(invoice), generator);

        Result<GetInvoiceDocumentResponse> result = await handler.ExecuteAsync(invoice.Id, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Content.ShouldBe([1, 2, 3]);
        result.Value.FileName.ShouldBe("nota-000007.pdf");
        generator.ReceivedInvoice.ShouldBeSameAs(invoice);
    }

    [Fact]
    public async Task Execute_WhenInvoiceIsOpen_ShouldReturnConflictWithoutGeneratingDocument()
    {
        InvoiceEntity invoice = CreateInvoice();
        var generator = new FakeDocumentGenerator();
        var handler = new GetInvoiceDocumentHandler(new FakeInvoiceRepository(invoice), generator);

        Result<GetInvoiceDocumentResponse> result = await handler.ExecuteAsync(invoice.Id, CancellationToken.None);

        result.IsSuccess.ShouldBeFalse();
        result.Error!.Code.ShouldBe("INVOICE_NOT_CLOSED");
        generator.ReceivedInvoice.ShouldBeNull();
    }

    [Fact]
    public async Task Execute_WhenInvoiceDoesNotExist_ShouldReturnNotFound()
    {
        var handler = new GetInvoiceDocumentHandler(new FakeInvoiceRepository(null), new FakeDocumentGenerator());

        Result<GetInvoiceDocumentResponse> result = await handler.ExecuteAsync(Guid.NewGuid(), CancellationToken.None);

        result.IsSuccess.ShouldBeFalse();
        result.Error!.Code.ShouldBe("INVOICE_NOT_FOUND");
    }

    [Fact]
    public async Task Execute_WhenGeneratorFails_ShouldReturnGenerationError()
    {
        InvoiceEntity invoice = CreateInvoice();
        invoice.Close(DateTimeOffset.UtcNow);
        var handler = new GetInvoiceDocumentHandler(new FakeInvoiceRepository(invoice), new FailingDocumentGenerator());

        Result<GetInvoiceDocumentResponse> result = await handler.ExecuteAsync(invoice.Id, CancellationToken.None);

        result.IsSuccess.ShouldBeFalse();
        result.Error!.Code.ShouldBe("PDF_GENERATION_ERROR");
    }

    private static InvoiceEntity CreateInvoice()
    {
        InvoiceEntity invoice = InvoiceEntity.Create(Guid.NewGuid(), 7, DateTimeOffset.UtcNow);
        invoice.AddItem(Guid.NewGuid(), "PROD-001", "Produto", 2);
        return invoice;
    }

    private sealed class FakeDocumentGenerator : IInvoiceDocumentGenerator
    {
        public InvoiceEntity? ReceivedInvoice { get; private set; }
        public byte[] Generate(InvoiceEntity invoice) { ReceivedInvoice = invoice; return [1, 2, 3]; }
    }

    private sealed class FailingDocumentGenerator : IInvoiceDocumentGenerator
    {
        public byte[] Generate(InvoiceEntity invoice) => throw new InvalidOperationException("Falha simulada.");
    }

    private sealed class FakeInvoiceRepository(InvoiceEntity? invoice) : IInvoiceRepository
    {
        public Task<InvoiceEntity?> GetByIdAsync(Guid invoiceId, CancellationToken cancellationToken) => Task.FromResult(invoice);
        public Task<IReadOnlyCollection<InvoiceEntity>> ListAsync(InvoiceStatus? status, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyCollection<InvoiceEntity>>([]);
        public Task AddAsync(InvoiceEntity value, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
