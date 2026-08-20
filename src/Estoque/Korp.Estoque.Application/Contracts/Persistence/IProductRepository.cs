using Korp.Estoque.Domain.Entities;

namespace Korp.Estoque.Application.Contracts.Persistence;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<Product>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken);

    Task<Paged<Product>> ListAsync(int pageNumber, int pageSize, CancellationToken cancellationToken);

    Task<IReadOnlyList<Product>> ListAvailableAsync(CancellationToken cancellationToken);

    Task<bool> TryAddAsync(Product product, CancellationToken cancellationToken);
}
