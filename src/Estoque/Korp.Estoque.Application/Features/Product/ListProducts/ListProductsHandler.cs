using FluentValidation.Results;
using Korp.Estoque.Application.Abstractions.Helpers;
using Korp.Estoque.Application.Contracts.Persistence;
using Korp.Estoque.Application.Contracts.UseCases;
using ProductEntity = Korp.Estoque.Domain.Entities.Product;

namespace Korp.Estoque.Application.Features.Product.ListProducts;

public sealed class ListProductsHandler(IValidator<ListProductsRequest> validator, IProductRepository productRepository, ILogger<ListProductsHandler>? logger = null) : IListProductsUseCase
{
    private readonly ILogger<ListProductsHandler> _logger = logger ?? NullLogger<ListProductsHandler>.Instance;

    public async Task<Result<Paged<ListProductsResponse>>> ExecuteAsync(ListProductsRequest request, CancellationToken ct)
    {
        _logger.LogInformation("Listando produtos. Página {PageNumber}, tamanho {PageSize}.", request.PageNumber, request.PageSize);

        ValidationResult validationResult = await validator.ValidateAsync(request, ct);

        if (!validationResult.IsValid)
        {
            _logger.LogWarning("Listagem de produtos rejeitada por validação.");
            return Result<Paged<ListProductsResponse>>.Failure(ValidationHelper.ToValidationError(validationResult));
        }

        Paged<ProductEntity> products = await productRepository.ListAsync(request.PageNumber, request.PageSize, ct);

        List<ListProductsResponse> items = products.Items
            .Select(product => new ListProductsResponse(
                product.Id,
                product.Code,
                product.Description,
                product.Balance))
            .ToList();

        _logger.LogInformation("Listagem de produtos concluída com {ProductCount} itens.", items.Count);
        return Result<Paged<ListProductsResponse>>.Success(new Paged<ListProductsResponse>(items, products.TotalRecords, products.PageNumber, products.PageSize));
    }
}
