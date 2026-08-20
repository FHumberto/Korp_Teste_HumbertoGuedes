using Korp.Faturamento.Domain.Enums;
using Korp.Faturamento.Infrastructure.Persistence.Repositories;
using Korp.Faturamento.IntegrationTests.Infrastructure;
using InvoiceEntity = Korp.Faturamento.Domain.Entities.Invoice;

namespace Korp.Faturamento.IntegrationTests.Persistence;

[Collection(FaturamentoIntegrationCollection.Name)]
public sealed class InvoicePersistenceTests(FaturamentoDatabaseFixture fixture) : IAsyncLifetime
{
    public Task InitializeAsync() => fixture.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    // Atende ao requisito de conexão real com banco: comprova que a nota e seus múltiplos itens persistem fisicamente no SQL Server.
    [Fact]
    public async Task AddAndGet_WhenInvoiceHasMultipleItems_ShouldPersistAggregate()
    {
        await using BillingDbContext writeContext = fixture.CreateBillingDbContext();
        var repository = new InvoiceRepository(writeContext);
        InvoiceEntity invoice = InvoiceEntity.Create(Guid.NewGuid(), 1, DateTimeOffset.UtcNow);
        invoice.AddItem(Guid.NewGuid(), "PROD-001", "Produto 1", 2);
        invoice.AddItem(Guid.NewGuid(), "PROD-002", "Produto 2", 3);

        await repository.AddAsync(invoice, CancellationToken.None);

        await using BillingDbContext readContext = fixture.CreateBillingDbContext();
        InvoiceEntity? persisted = await new InvoiceRepository(readContext)
            .GetByIdAsync(invoice.Id, CancellationToken.None);
        persisted.ShouldNotBeNull();
        persisted.Status.ShouldBe(InvoiceStatus.Open);
        persisted.Items.Count.ShouldBe(2);
    }

    // Atende ao requisito de numeração sequencial no backend: valida a sequence nativa do SQL Server, sem simulação em memória.
    [Fact]
    public async Task GetNext_WhenCalledTwice_ShouldReturnSequentialUniqueNumbers()
    {
        await using BillingDbContext dbContext = fixture.CreateBillingDbContext();
        var generator = new InvoiceNumberGenerator(dbContext);

        long first = await generator.GetNextAsync(CancellationToken.None);
        long second = await generator.GetNextAsync(CancellationToken.None);

        first.ShouldBe(1);
        second.ShouldBe(2);
    }
}
