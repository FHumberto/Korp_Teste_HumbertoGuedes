using Korp.Estoque.Application.Features.Product.GetProductsByIds;

namespace Korp.Estoque.Application.Contracts.UseCases;

public interface IGetProductsByIdsUseCase
{
    Task<Result<IReadOnlyCollection<GetProductsByIdsResponse>>> ExecuteAsync(GetProductsByIdsRequest request, CancellationToken cancellationToken);
}
