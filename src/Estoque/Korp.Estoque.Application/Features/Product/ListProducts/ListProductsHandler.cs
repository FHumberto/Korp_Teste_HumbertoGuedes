using FluentValidation;
using FluentValidation.Results;
using Korp.Estoque.Application.Abstractions.Helpers;
using Korp.Estoque.Application.Abstractions.Wrappers;
using Korp.Estoque.Application.Contracts.Persistence;
using Korp.Estoque.Application.Contracts.UseCases;
using Korp.Estoque.Domain.Abstractions.Types;
using ProductEntity = Korp.Estoque.Domain.Entities.Product;

namespace Korp.Estoque.Application.Features.Product.ListProducts;

public sealed class ListProductsHandler(IValidator<ListProductsRequest> validator, IProductRepository productRepository) : IListProductsUseCase
{
    public async Task<Result<Paged<ListProductsResponse>>> ExecuteAsync(ListProductsRequest request, CancellationToken ct)
    {
        ValidationResult validationResult = await validator.ValidateAsync(request, ct);

        if (!validationResult.IsValid)
            return Result<Paged<ListProductsResponse>>.Failure(ValidationHelper.ToValidationError(validationResult));

        Paged<ProductEntity> products = await productRepository.ListAsync(request.PageNumber, request.PageSize, ct);

        List<ListProductsResponse> items = products.Items
            .Select(product => new ListProductsResponse(
                product.Id,
                product.Code,
                product.Description,
                product.Balance))
            .ToList();

        return Result<Paged<ListProductsResponse>>.Success(new Paged<ListProductsResponse>(items, products.TotalRecords, products.PageNumber, products.PageSize));
    }
}
