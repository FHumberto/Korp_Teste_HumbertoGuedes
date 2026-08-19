using Korp.Estoque.Domain.Entities;

namespace Korp.Estoque.Application.Contracts.Persistence;

public interface IProductRepository
{
    Task<bool> TryAddAsync(Product product, CancellationToken cancellationToken);

    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}
