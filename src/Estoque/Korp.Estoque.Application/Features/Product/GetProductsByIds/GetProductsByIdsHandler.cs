using FluentValidation.Results;
using Korp.Estoque.Application.Abstractions.Helpers;
using Korp.Estoque.Application.Contracts.Persistence;
using Korp.Estoque.Application.Contracts.UseCases;
using Korp.Estoque.Domain.Entities.Errors;
using ProductEntity = Korp.Estoque.Domain.Entities.Product;

namespace Korp.Estoque.Application.Features.Product.GetProductsByIds;

public sealed class GetProductsByIdsHandler(IValidator<GetProductsByIdsRequest> validator, IProductRepository productRepository, ILogger<GetProductsByIdsHandler>? logger = null) : IGetProductsByIdsUseCase
{
    private readonly ILogger<GetProductsByIdsHandler> _logger = logger ?? NullLogger<GetProductsByIdsHandler>.Instance;

    public async Task<Result<IReadOnlyCollection<GetProductsByIdsResponse>>> ExecuteAsync(GetProductsByIdsRequest request, CancellationToken ct)
    {
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("Consultando {ProductCount} produtos por identificador.", request.ProductIds?.Count ?? 0);
        ValidationResult validationResult = await validator.ValidateAsync(request, ct);

        if (!validationResult.IsValid)
        {
            _logger.LogWarning("Consulta de produtos por identificadores rejeitada por validação.");
            return Result<IReadOnlyCollection<GetProductsByIdsResponse>>.Failure(ValidationHelper.ToValidationError(validationResult));
        }

        IReadOnlyCollection<Guid> productIds = request.ProductIds!;
        IReadOnlyList<ProductEntity> products = await productRepository.GetByIdsAsync(productIds, ct);

        if (products.Count != productIds.Count)
        {
            _logger.LogWarning("Um ou mais produtos solicitados não foram encontrados.");
            return Result<IReadOnlyCollection<GetProductsByIdsResponse>>.Failure(ProductErrors.NotFound);
        }

        var productsById = products.ToDictionary(product => product.Id);

        List<GetProductsByIdsResponse> response = productIds
            .Select(productId => productsById[productId])
            .Select(product => new GetProductsByIdsResponse(product.Id, product.Code, product.Description, product.Balance))
            .ToList();

        _logger.LogInformation("Consulta por identificadores concluída com {ProductCount} produtos.", response.Count);
        return Result<IReadOnlyCollection<GetProductsByIdsResponse>>.Success(response);
    }
}
