using FluentValidation;
using FluentValidation.Results;
using Korp.Estoque.Application.Abstractions.Helpers;
using Korp.Estoque.Application.Abstractions.Wrappers;
using Korp.Estoque.Application.Contracts.Persistence;
using Korp.Estoque.Application.Contracts.UseCases;
using Korp.Estoque.Domain.Entities.Errors;
using ProductEntity = Korp.Estoque.Domain.Entities.Product;

namespace Korp.Estoque.Application.Features.Product.CreateProduct;

public sealed class CreateProductHandler(IValidator<CreateProductRequest> validator, IProductRepository productRepository) : ICreateProductUseCase
{
    public async Task<Result<CreateProductResponse>> ExecuteAsync(CreateProductRequest request, CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
            return Result<CreateProductResponse>.Failure(ValidationHelper.ToValidationError(validationResult));

        request.Normalize();

        ProductEntity product = ProductEntity.Create(Guid.NewGuid(), request.Code, request.Description, request.InitialBalance, DateTime.UtcNow);

        bool productCreated = await productRepository.TryAddAsync(product, cancellationToken);

        return !productCreated
            ? Result<CreateProductResponse>.Failure(ProductErrors.CodeAlreadyExists)
            : Result<CreateProductResponse>.Success(new CreateProductResponse(product.Id, product.Code, product.Description, product.Balance, product.CreatedAt));
    }
}
