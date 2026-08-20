namespace Korp.Estoque.Application.Features.Product.GetProduct;

public sealed record GetProductResponse
(
    Guid Id,
    string Code,
    string Description,
    int Balance,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);
