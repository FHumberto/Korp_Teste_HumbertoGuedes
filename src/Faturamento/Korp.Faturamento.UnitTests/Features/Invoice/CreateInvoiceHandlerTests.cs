using Korp.Faturamento.Application.Abstractions.Wrappers;
using Korp.Faturamento.Application.Contracts.Gateways;
using Korp.Faturamento.Application.Contracts.Persistence;
using Korp.Faturamento.Application.Features.Invoice.CreateInvoice;
using Korp.Faturamento.Domain.Enums;
using InvoiceEntity = Korp.Faturamento.Domain.Entities.Invoice;

namespace Korp.Faturamento.UnitTests.Features.Invoice;

public sealed class CreateInvoiceHandlerTests
{
    [Fact]
    public async Task Execute_WhenRequestIsValid_ShouldCreateOpenInvoiceWithInventorySnapshots()
    {
        Guid productId = Guid.NewGuid();
        DateTimeOffset now = new(2026, 8, 20, 12, 0, 0, TimeSpan.Zero);
        var repository = new FakeInvoiceRepository();
        CreateInvoiceHandler handler = CreateHandler(
            new FakeInventoryGateway([new(productId, "PROD-001", "Produto confiável", 10)]),
            repository,
            new FixedTimeProvider(now));
        var request = new CreateInvoiceRequest { Items = [new(productId, 2)] };

        Result<CreateInvoiceResponse> result = await handler.ExecuteAsync(request, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Number.ShouldBe(42);
        result.Value.Status.ShouldBe("open");
        result.Value.CreatedAt.ShouldBe(now);
        result.Value.ClosedAt.ShouldBeNull();
        result.Value.Items.Single().ProductCode.ShouldBe("PROD-001");
        result.Value.Items.Single().ProductDescription.ShouldBe("Produto confiável");
        repository.AddedInvoice.ShouldNotBeNull();
    }

    [Fact]
    public async Task Execute_WhenProductDoesNotExist_ShouldReturnProductNotFound()
    {
        var repository = new FakeInvoiceRepository();
        CreateInvoiceHandler handler = CreateHandler(new FakeInventoryGateway([]), repository, TimeProvider.System);
        var request = new CreateInvoiceRequest { Items = [new(Guid.NewGuid(), 1)] };

        Result<CreateInvoiceResponse> result = await handler.ExecuteAsync(request, CancellationToken.None);

        result.IsSuccess.ShouldBeFalse();
        result.Error!.Code.ShouldBe("PRODUCT_NOT_FOUND");
        repository.AddedInvoice.ShouldBeNull();
    }

    [Fact]
    public async Task Execute_WhenInventoryIsUnavailable_ShouldReturnServiceUnavailable()
    {
        var repository = new FakeInvoiceRepository();
        CreateInvoiceHandler handler = CreateHandler(new FakeInventoryGateway(new InventoryUnavailableException("Falha")), repository, TimeProvider.System);
        var request = new CreateInvoiceRequest { Items = [new(Guid.NewGuid(), 1)] };

        Result<CreateInvoiceResponse> result = await handler.ExecuteAsync(request, CancellationToken.None);

        result.IsSuccess.ShouldBeFalse();
        result.Error!.Code.ShouldBe("INVENTORY_UNAVAILABLE");
        repository.AddedInvoice.ShouldBeNull();
    }

    private static CreateInvoiceHandler CreateHandler(
        IInventoryGateway inventoryGateway,
        IInvoiceRepository repository,
        TimeProvider timeProvider) => new(
            new CreateInvoiceValidator(),
            inventoryGateway,
            new FakeInvoiceNumberGenerator(),
            repository,
            timeProvider);

    private sealed class FakeInvoiceRepository : IInvoiceRepository
    {
        public InvoiceEntity? AddedInvoice { get; private set; }

        public Task<InvoiceEntity?> GetByIdAsync(Guid invoiceId, CancellationToken cancellationToken) => Task.FromResult<InvoiceEntity?>(null);

        public Task<IReadOnlyCollection<InvoiceEntity>> ListAsync(InvoiceStatus? status, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyCollection<InvoiceEntity>>([]);

        public Task AddAsync(InvoiceEntity invoice, CancellationToken cancellationToken)
        {
            AddedInvoice = invoice;
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class FakeInvoiceNumberGenerator : IInvoiceNumberGenerator
    {
        public Task<long> GetNextAsync(CancellationToken cancellationToken) => Task.FromResult(42L);
    }

    private sealed class FakeInventoryGateway : IInventoryGateway
    {
        private readonly IReadOnlyCollection<InventoryProduct> _products;
        private readonly Exception? _exception;

        public FakeInventoryGateway(IReadOnlyCollection<InventoryProduct> products) => _products = products;

        public FakeInventoryGateway(Exception exception)
        {
            _products = [];
            _exception = exception;
        }

        public Task<IReadOnlyCollection<InventoryProduct>> GetProductsByIdsAsync(
            IReadOnlyCollection<Guid> productIds,
            CancellationToken cancellationToken) => _exception is not null ? throw _exception : Task.FromResult(_products);

        public Task<DebitStockResult> DebitAsync(
            DebitStockCommand command,
            string idempotencyKey,
            CancellationToken cancellationToken) => Task.FromResult(DebitStockResult.Succeeded);
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
