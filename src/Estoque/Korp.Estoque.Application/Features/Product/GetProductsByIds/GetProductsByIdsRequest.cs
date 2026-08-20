namespace Korp.Estoque.Application.Features.Product.GetProductsByIds;

public sealed class GetProductsByIdsRequest
{
    public IReadOnlyCollection<Guid> ProductIds { get; init; } = [];
}
