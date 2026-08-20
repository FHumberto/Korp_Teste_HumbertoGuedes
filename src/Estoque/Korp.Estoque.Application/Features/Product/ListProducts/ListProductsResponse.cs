namespace Korp.Estoque.Application.Features.Product.ListProducts;

public sealed record ListProductsResponse
(
    Guid Id,
    string Code,
    string Description,
    int Balance
);
