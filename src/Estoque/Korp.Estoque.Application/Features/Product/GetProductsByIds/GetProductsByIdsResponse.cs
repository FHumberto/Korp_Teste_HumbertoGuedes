namespace Korp.Estoque.Application.Features.Product.GetProductsByIds;

public sealed record GetProductsByIdsResponse
(
    Guid Id,
    string Code,
    string Description,
    int Balance
);
