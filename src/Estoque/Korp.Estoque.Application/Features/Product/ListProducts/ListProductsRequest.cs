namespace Korp.Estoque.Application.Features.Product.ListProducts;

public sealed class ListProductsRequest
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
