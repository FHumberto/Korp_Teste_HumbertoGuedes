using Korp.Estoque.Domain.Entities;
using Korp.Estoque.Infrastructure.Persistence;
using Korp.Faturamento.Application.Abstractions.Wrappers;
using Korp.Faturamento.Application.Features.Invoice.CloseInvoice;
using Korp.Faturamento.Application.Features.Invoice.CreateInvoice;
using Korp.Faturamento.Infrastructure.Persistence.Repositories;
using Korp.Faturamento.IntegrationTests.Infrastructure;

namespace Korp.Faturamento.IntegrationTests.Closing;

[Collection(FaturamentoIntegrationCollection.Name)]
public sealed class CrossServiceClosingTests(FaturamentoDatabaseFixture fixture) : IAsyncLifetime
{
    private SqlServerInventoryGateway _inventoryGateway = null!;

    public async Task InitializeAsync()
    {
        await fixture.ResetAsync();
        _inventoryGateway = new SqlServerInventoryGateway(fixture);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    // Atende à idempotência e recuperação de falha: uma resposta perdida mantém a nota aberta e a retentativa não baixa o estoque novamente.
    [Fact]
    public async Task Close_WhenInventoryResponseIsLost_ShouldRetryWithoutSecondDebit()
    {
        Guid productId = Guid.NewGuid();
        await SeedProductAsync(productId, "PROD-001", 1);
        Guid invoiceId = await CreateInvoiceAsync([new(productId, 1)]);
        _inventoryGateway.LoseNextSuccessfulResponse = true;

        Result<CloseInvoiceResponse> firstAttempt = await CloseInvoiceAsync(invoiceId);
        firstAttempt.IsSuccess.ShouldBeFalse();
        firstAttempt.Error!.Code.ShouldBe("INVENTORY_UNAVAILABLE");
        (await GetBalanceAsync(productId)).ShouldBe(0);
        (await GetInvoiceStatusAsync(invoiceId)).ShouldBe("open");

        Result<CloseInvoiceResponse> retry = await CloseInvoiceAsync(invoiceId);
        retry.IsSuccess.ShouldBeTrue();
        (await GetBalanceAsync(productId)).ShouldBe(0);
        (await GetInvoiceStatusAsync(invoiceId)).ShouldBe("closed");
        _inventoryGateway.ReceivedIdempotencyKeys.Count.ShouldBe(2);
        _inventoryGateway.ReceivedIdempotencyKeys.Distinct().ShouldHaveSingleItem()
            .ShouldBe($"invoice:{invoiceId}:close:v1");
    }

    // Atende à atomicidade da baixa: se um produto não tiver saldo, nenhum dos demais itens da nota pode ser debitado.
    [Fact]
    public async Task Close_WhenOneItemHasInsufficientStock_ShouldRollbackEveryDebit()
    {
        Guid availableProductId = Guid.NewGuid();
        Guid unavailableProductId = Guid.NewGuid();
        await SeedProductAsync(availableProductId, "PROD-001", 5);
        await SeedProductAsync(unavailableProductId, "PROD-002", 0);
        Guid invoiceId = await CreateInvoiceAsync([
            new(availableProductId, 2),
            new(unavailableProductId, 1)
        ]);

        Result<CloseInvoiceResponse> result = await CloseInvoiceAsync(invoiceId);

        result.IsSuccess.ShouldBeFalse();
        result.Error!.Code.ShouldBe("INSUFFICIENT_STOCK");
        (await GetBalanceAsync(availableProductId)).ShouldBe(5);
        (await GetBalanceAsync(unavailableProductId)).ShouldBe(0);
        (await GetInvoiceStatusAsync(invoiceId)).ShouldBe("open");
    }

    // Atende ao tratamento de concorrência: duas notas disputando a última unidade produzem um sucesso, um conflito e saldo final zero.
    [Fact]
    public async Task Close_WhenTwoInvoicesCompeteForLastUnit_ShouldCloseOnlyOne()
    {
        Guid productId = Guid.NewGuid();
        await SeedProductAsync(productId, "PROD-001", 1);
        Guid firstInvoiceId = await CreateInvoiceAsync([new(productId, 1)]);
        Guid secondInvoiceId = await CreateInvoiceAsync([new(productId, 1)]);

        Result<CloseInvoiceResponse>[] results = await Task.WhenAll(
            CloseInvoiceAsync(firstInvoiceId),
            CloseInvoiceAsync(secondInvoiceId));

        results.Count(result => result.IsSuccess).ShouldBe(1);
        results.Count(result => !result.IsSuccess && result.Error!.Code == "INSUFFICIENT_STOCK").ShouldBe(1);
        (await GetBalanceAsync(productId)).ShouldBe(0);
        string[] statuses = [
            await GetInvoiceStatusAsync(firstInvoiceId),
            await GetInvoiceStatusAsync(secondInvoiceId)
        ];
        statuses.Count(status => status == "closed").ShouldBe(1);
        statuses.Count(status => status == "open").ShouldBe(1);
    }

    private async Task<Guid> CreateInvoiceAsync(IReadOnlyCollection<CreateInvoiceItemRequest> items)
    {
        await using BillingDbContext dbContext = fixture.CreateBillingDbContext();
        var handler = new CreateInvoiceHandler(
            new CreateInvoiceValidator(),
            _inventoryGateway,
            new InvoiceNumberGenerator(dbContext),
            new InvoiceRepository(dbContext),
            TimeProvider.System);
        Result<CreateInvoiceResponse> result = await handler.ExecuteAsync(
            new CreateInvoiceRequest { Items = items },
            CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
        return result.Value.Id;
    }

    private async Task<Result<CloseInvoiceResponse>> CloseInvoiceAsync(Guid invoiceId)
    {
        await using BillingDbContext dbContext = fixture.CreateBillingDbContext();
        var handler = new CloseInvoiceHandler(new InvoiceRepository(dbContext), _inventoryGateway, TimeProvider.System);
        return await handler.ExecuteAsync(invoiceId, CancellationToken.None);
    }

    private async Task SeedProductAsync(Guid productId, string code, int balance)
    {
        await using InventoryDbContext dbContext = fixture.CreateInventoryDbContext();
        dbContext.Products.Add(Product.Create(productId, code, $"Produto {code}", balance, DateTimeOffset.UtcNow));
        await dbContext.SaveChangesAsync();
    }

    private async Task<int> GetBalanceAsync(Guid productId)
    {
        await using InventoryDbContext dbContext = fixture.CreateInventoryDbContext();
        return await dbContext.Products.Where(product => product.Id == productId)
            .Select(product => product.Balance)
            .SingleAsync();
    }

    private async Task<string> GetInvoiceStatusAsync(Guid invoiceId)
    {
        await using BillingDbContext dbContext = fixture.CreateBillingDbContext();
        return (await new InvoiceRepository(dbContext).GetByIdAsync(invoiceId, CancellationToken.None))!
            .Status.ToString().ToLowerInvariant();
    }
}
