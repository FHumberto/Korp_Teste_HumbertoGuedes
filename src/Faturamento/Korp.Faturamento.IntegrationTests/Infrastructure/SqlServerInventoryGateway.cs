using System.Collections.Concurrent;
using Korp.Estoque.Application.Abstractions.Wrappers;
using Korp.Estoque.Application.Features.Stock.DebitStock;
using Korp.Estoque.Infrastructure.Persistence;
using Korp.Estoque.Infrastructure.Persistence.Repositories;
using Korp.Faturamento.Application.Contracts.Gateways;
using Microsoft.EntityFrameworkCore;
using BillingDebitCommand = Korp.Faturamento.Application.Contracts.Gateways.DebitStockCommand;
using InventoryDebitCommand = Korp.Estoque.Application.Features.Stock.DebitStock.DebitStockCommand;

namespace Korp.Faturamento.IntegrationTests.Infrastructure;

public sealed class SqlServerInventoryGateway(FaturamentoDatabaseFixture fixture) : IInventoryGateway
{
    public bool LoseNextSuccessfulResponse { get; set; }
    public ConcurrentBag<string> ReceivedIdempotencyKeys { get; } = [];

    public async Task<IReadOnlyCollection<InventoryProduct>> GetProductsByIdsAsync(IReadOnlyCollection<Guid> productIds, CancellationToken cancellationToken)
    {
        await using InventoryDbContext dbContext = fixture.CreateInventoryDbContext();
        return await dbContext.Products.AsNoTracking()
            .Where(product => productIds.Contains(product.Id))
            .Select(product => new InventoryProduct(product.Id, product.Code, product.Description, product.Balance))
            .ToArrayAsync(cancellationToken);
    }

    public async Task<DebitStockResult> DebitAsync(BillingDebitCommand command, string idempotencyKey, CancellationToken cancellationToken)
    {
        ReceivedIdempotencyKeys.Add(idempotencyKey);
        await using InventoryDbContext dbContext = fixture.CreateInventoryDbContext();
        var handler = new DebitStockHandler(
            new DebitStockCommandValidator(),
            new StockDebitRepository(dbContext),
            TimeProvider.System);
        var inventoryCommand = new InventoryDebitCommand(
            idempotencyKey,
            command.InvoiceId,
            command.Items.Select(item => new DebitStockItemRequest(item.ProductId, item.Quantity)).ToArray());

        Result<DebitStockResponse> result = await handler.ExecuteAsync(inventoryCommand, cancellationToken);
        if (result.IsSuccess)
        {
            if (LoseNextSuccessfulResponse)
            {
                LoseNextSuccessfulResponse = false;
                throw new InventoryUnavailableException("A baixa foi confirmada no Estoque, mas a resposta foi perdida.");
            }

            return DebitStockResult.Succeeded;
        }

        return result.Error!.Code switch
        {
            "PRODUCT_NOT_FOUND" => new DebitStockResult(DebitStockStatus.ProductNotFound),
            "INSUFFICIENT_STOCK" => new DebitStockResult(DebitStockStatus.InsufficientStock),
            "IDEMPOTENCY_CONFLICT" => new DebitStockResult(DebitStockStatus.IdempotencyConflict),
            _ => throw new InvalidOperationException($"Erro inesperado do Estoque: {result.Error.Code}.")
        };
    }
}
