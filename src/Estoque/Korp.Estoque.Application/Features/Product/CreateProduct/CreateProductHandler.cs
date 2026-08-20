using FluentValidation.Results;
using Korp.Estoque.Application.Abstractions.Helpers;
using Korp.Estoque.Application.Contracts.Persistence;
using Korp.Estoque.Application.Contracts.UseCases;
using Korp.Estoque.Domain.Entities.Errors;
using ProductEntity = Korp.Estoque.Domain.Entities.Product;

namespace Korp.Estoque.Application.Features.Product.CreateProduct;

public sealed class CreateProductHandler(IValidator<CreateProductRequest> validator, IProductRepository productRepository, ILogger<CreateProductHandler>? logger = null) : ICreateProductUseCase
{
    private readonly ILogger<CreateProductHandler> _logger = logger ?? NullLogger<CreateProductHandler>.Instance;

    public async Task<Result<CreateProductResponse>> ExecuteAsync(CreateProductRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando cadastro do produto com código {ProductCode}.", request.Code);
        ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            _logger.LogWarning("Cadastro do produto rejeitado por validação. Código {ProductCode}.", request.Code);
            return Result<CreateProductResponse>.Failure(ValidationHelper.ToValidationError(validationResult));
        }

        request.Normalize();

        ProductEntity product = ProductEntity.Create(Guid.NewGuid(), request.Code, request.Description, request.InitialBalance, DateTime.UtcNow);

        bool productCreated = await productRepository.TryAddAsync(product, cancellationToken);

        if (!productCreated)
        {
            _logger.LogWarning("Cadastro rejeitado porque o código {ProductCode} já existe.", product.Code);
            return Result<CreateProductResponse>.Failure(ProductErrors.CodeAlreadyExists);
        }

        _logger.LogInformation("Produto {ProductId} cadastrado com sucesso.", product.Id);
        return Result<CreateProductResponse>.Success(new CreateProductResponse(product.Id, product.Code, product.Description, product.Balance, product.CreatedAt));
    }
}
