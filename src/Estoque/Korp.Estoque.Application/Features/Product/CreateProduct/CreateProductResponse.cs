namespace Korp.Estoque.Application.Features.Product.CreateProduct;

public sealed record CreateProductResponse
(
    Guid Id,
    string Code,
    string Description,
    int Balance,
    DateTimeOffset CreatedAt
);
