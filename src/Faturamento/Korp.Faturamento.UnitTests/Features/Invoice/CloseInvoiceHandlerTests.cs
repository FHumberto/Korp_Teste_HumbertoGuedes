using Korp.Faturamento.Application.Abstractions.Wrappers;
using Korp.Faturamento.Application.Contracts.Gateways;
using Korp.Faturamento.Application.Contracts.Persistence;
using Korp.Faturamento.Application.Features.Invoice.CloseInvoice;
using Korp.Faturamento.Domain.Enums;
using Shouldly;
using InvoiceEntity = Korp.Faturamento.Domain.Entities.Invoice;

namespace Korp.Faturamento.UnitTests.Features.Invoice;

public sealed class CloseInvoiceHandlerTests
{
    private static readonly DateTimeOffset ClosedAt = new(2026, 8, 20, 15, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Execute_WhenInventoryConfirmsDebit_ShouldCloseAndPersistInvoice()
    {
        InvoiceEntity invoice = CreateOpenInvoice();
        var repository = new FakeInvoiceRepository(invoice);
        var gateway = new FakeInventoryGateway(DebitStockResult.Succeeded);
        CloseInvoiceHandler handler = CreateHandler(repository, gateway);

        Result<CloseInvoiceResponse> result = await handler.ExecuteAsync(invoice.Id, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Status.ShouldBe("closed");
        result.Value.ClosedAt.ShouldBe(ClosedAt);
        repository.SaveCount.ShouldBe(1);
        gateway.ReceivedKey.ShouldBe($"invoice:{invoice.Id}:close:v1");
        gateway.ReceivedCommand!.Items.ShouldHaveSingleItem().Quantity.ShouldBe(2);
    }

    [Theory]
    [InlineData(DebitStockStatus.InsufficientStock, "INSUFFICIENT_STOCK")]
    [InlineData(DebitStockStatus.IdempotencyConflict, "IDEMPOTENCY_CONFLICT")]
    [InlineData(DebitStockStatus.ProductNotFound, "PRODUCT_NOT_FOUND")]
    public async Task Execute_WhenInventoryRejectsDebit_ShouldKeepInvoiceOpen(
        DebitStockStatus status,
        string expectedCode)
    {
        InvoiceEntity invoice = CreateOpenInvoice();
        var repository = new FakeInvoiceRepository(invoice);
        CloseInvoiceHandler handler = CreateHandler(repository, new FakeInventoryGateway(new DebitStockResult(status)));

        Result<CloseInvoiceResponse> result = await handler.ExecuteAsync(invoice.Id, CancellationToken.None);

        result.IsSuccess.ShouldBeFalse();
        result.Error!.Code.ShouldBe(expectedCode);
        invoice.Status.ShouldBe(InvoiceStatus.Open);
        repository.SaveCount.ShouldBe(0);
    }

    [Fact]
    public async Task Execute_WhenInventoryIsUnavailable_ShouldKeepInvoiceOpen()
    {
        InvoiceEntity invoice = CreateOpenInvoice();
        var repository = new FakeInvoiceRepository(invoice);
        CloseInvoiceHandler handler = CreateHandler(repository, new FakeInventoryGateway(new InventoryUnavailableException("Falha")));

        Result<CloseInvoiceResponse> result = await handler.ExecuteAsync(invoice.Id, CancellationToken.None);

        result.IsSuccess.ShouldBeFalse();
        result.Error!.Code.ShouldBe("INVENTORY_UNAVAILABLE");
        invoice.Status.ShouldBe(InvoiceStatus.Open);
        repository.SaveCount.ShouldBe(0);
    }

    [Fact]
    public async Task Execute_WhenInvoiceIsAlreadyClosed_ShouldNotCallInventory()
    {
        InvoiceEntity invoice = CreateOpenInvoice();
        invoice.Close(ClosedAt.AddMinutes(-1));
        var gateway = new FakeInventoryGateway(DebitStockResult.Succeeded);
        CloseInvoiceHandler handler = CreateHandler(new FakeInvoiceRepository(invoice), gateway);

        Result<CloseInvoiceResponse> result = await handler.ExecuteAsync(invoice.Id, CancellationToken.None);

        result.IsSuccess.ShouldBeFalse();
        result.Error!.Code.ShouldBe("INVOICE_ALREADY_CLOSED");
        gateway.CallCount.ShouldBe(0);
    }

    [Fact]
    public async Task Execute_WhenRetriedAfterInventoryFailure_ShouldReuseKeyAndCloseOnce()
    {
        InvoiceEntity invoice = CreateOpenInvoice();
        var repository = new FakeInvoiceRepository(invoice);
        var gateway = new FakeInventoryGateway(
            new InventoryUnavailableException("Resposta perdida"),
            DebitStockResult.Succeeded);
        CloseInvoiceHandler handler = CreateHandler(repository, gateway);

        Result<CloseInvoiceResponse> firstResult = await handler.ExecuteAsync(invoice.Id, CancellationToken.None);
        Result<CloseInvoiceResponse> secondResult = await handler.ExecuteAsync(invoice.Id, CancellationToken.None);

        firstResult.IsSuccess.ShouldBeFalse();
        secondResult.IsSuccess.ShouldBeTrue();
        gateway.ReceivedKeys.Count.ShouldBe(2);
        gateway.ReceivedKeys.Distinct().ShouldHaveSingleItem().ShouldBe($"invoice:{invoice.Id}:close:v1");
        repository.SaveCount.ShouldBe(1);
    }

    private static CloseInvoiceHandler CreateHandler(FakeInvoiceRepository repository, FakeInventoryGateway gateway) =>
        new(repository, gateway, new FixedTimeProvider(ClosedAt));

    private static InvoiceEntity CreateOpenInvoice()
    {
        InvoiceEntity invoice = InvoiceEntity.Create(Guid.NewGuid(), 1, ClosedAt.AddHours(-1));
        invoice.AddItem(Guid.NewGuid(), "PROD-001", "Produto", 2);
        return invoice;
    }

    private sealed class FakeInvoiceRepository(InvoiceEntity? invoice) : IInvoiceRepository
    {
        public int SaveCount { get; private set; }
        public Task<InvoiceEntity?> GetByIdAsync(Guid invoiceId, CancellationToken cancellationToken) => Task.FromResult(invoice);
        public Task<IReadOnlyCollection<InvoiceEntity>> ListAsync(InvoiceStatus? status, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyCollection<InvoiceEntity>>([]);
        public Task AddAsync(InvoiceEntity value, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            SaveCount++;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeInventoryGateway(params object[] outcomes) : IInventoryGateway
    {
        private readonly Queue<object> _outcomes = new(outcomes);
        public int CallCount { get; private set; }
        public DebitStockCommand? ReceivedCommand { get; private set; }
        public string? ReceivedKey { get; private set; }
        public List<string> ReceivedKeys { get; } = [];

        public Task<IReadOnlyCollection<InventoryProduct>> GetProductsByIdsAsync(IReadOnlyCollection<Guid> productIds, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyCollection<InventoryProduct>>([]);

        public Task<DebitStockResult> DebitAsync(DebitStockCommand command, string idempotencyKey, CancellationToken cancellationToken)
        {
            CallCount++;
            ReceivedCommand = command;
            ReceivedKey = idempotencyKey;
            ReceivedKeys.Add(idempotencyKey);
            object outcome = _outcomes.Dequeue();

            return outcome is Exception exception ? throw exception : Task.FromResult((DebitStockResult)outcome);
        }
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
