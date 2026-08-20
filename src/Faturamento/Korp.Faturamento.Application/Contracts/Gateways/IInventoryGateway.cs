namespace Korp.Faturamento.Application.Contracts.Gateways;

public interface IInventoryGateway
{
    Task<IReadOnlyCollection<InventoryProduct>> GetProductsByIdsAsync(
        IReadOnlyCollection<Guid> productIds,
        CancellationToken cancellationToken);
}

public sealed record InventoryProduct(Guid Id, string Code, string Description, int Balance);
