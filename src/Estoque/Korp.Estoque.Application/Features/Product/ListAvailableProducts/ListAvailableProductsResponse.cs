namespace Korp.Estoque.Application.Features.Product.ListAvailableProducts;

public sealed record ListAvailableProductsResponse(Guid Id, string Code, string Description, int Balance);
