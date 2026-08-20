using FluentValidation;
using FluentValidation.Results;
using Korp.Estoque.Application.Abstractions.Helpers;
using Korp.Estoque.Application.Abstractions.Wrappers;
using Korp.Estoque.Application.Contracts.Persistence;
using Korp.Estoque.Application.Contracts.UseCases;
using Korp.Estoque.Domain.Entities.Errors;
using ProductEntity = Korp.Estoque.Domain.Entities.Product;

namespace Korp.Estoque.Application.Features.Product.GetProductsByIds;

public sealed class GetProductsByIdsHandler(IValidator<GetProductsByIdsRequest> validator, IProductRepository productRepository) : IGetProductsByIdsUseCase
{
    public async Task<Result<IReadOnlyCollection<GetProductsByIdsResponse>>> ExecuteAsync(GetProductsByIdsRequest request, CancellationToken ct)
    {
        ValidationResult validationResult = await validator.ValidateAsync(request, ct);

        if (!validationResult.IsValid)
            return Result<IReadOnlyCollection<GetProductsByIdsResponse>>.Failure(ValidationHelper.ToValidationError(validationResult));

        IReadOnlyList<ProductEntity> products = await productRepository.GetByIdsAsync(request.ProductIds, ct);

        if (products.Count != request.ProductIds.Count)
            return Result<IReadOnlyCollection<GetProductsByIdsResponse>>.Failure(ProductErrors.NotFound);

        var productsById = products.ToDictionary(product => product.Id);

        List<GetProductsByIdsResponse> response = request.ProductIds
            .Select(productId => productsById[productId])
            .Select(product => new GetProductsByIdsResponse(product.Id, product.Code, product.Description, product.Balance))
            .ToList();

        return Result<IReadOnlyCollection<GetProductsByIdsResponse>>.Success(response);
    }
}
