using Korp.Estoque.Application.Abstractions.Wrappers;
using Korp.Estoque.Application.Contracts.Persistence;
using Korp.Estoque.Application.Features.Product.GetProductsByIds;
using Korp.Estoque.Domain.Abstractions.Types;
using Korp.Estoque.Domain.Entities.Errors;
using Shouldly;
using ProductEntity = Korp.Estoque.Domain.Entities.Product;

namespace Korp.Estoque.UnitTests.Features.Product;

public sealed class GetProductsByIdsHandlerTests
{
    [Fact]
    public async Task Execute_WhenAllProductsExist_ShouldReturnProductsInRequestedOrder()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        ProductEntity firstProduct = ProductEntity.Create(Guid.NewGuid(), "PROD-001", "Primeiro produto", 10, now);
        ProductEntity secondProduct = ProductEntity.Create(Guid.NewGuid(), "PROD-002", "Segundo produto", 20, now);
        ProductRepositoryStub repository = new([firstProduct, secondProduct]);
        GetProductsByIdsHandler handler = new(new GetProductsByIdsValidator(), repository);
        GetProductsByIdsRequest request = new() { ProductIds = [secondProduct.Id, firstProduct.Id] };

        Result<IReadOnlyCollection<GetProductsByIdsResponse>> result = await handler.ExecuteAsync(
            request,
            CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Select(product => product.Id).ShouldBe([secondProduct.Id, firstProduct.Id]);
        GetProductsByIdsResponse response = result.Value.First();
        response.Code.ShouldBe(secondProduct.Code);
        response.Description.ShouldBe(secondProduct.Description);
        response.Balance.ShouldBe(secondProduct.Balance);
        repository.ReceivedIds.ShouldBe(request.ProductIds);
    }

    [Fact]
    public async Task Execute_WhenAnyProductDoesNotExist_ShouldReturnNotFound()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        ProductEntity product = ProductEntity.Create(Guid.NewGuid(), "PROD-001", "Produto", 10, now);
        ProductRepositoryStub repository = new([product]);
        GetProductsByIdsHandler handler = new(new GetProductsByIdsValidator(), repository);
        GetProductsByIdsRequest request = new() { ProductIds = [product.Id, Guid.NewGuid()] };

        Result<IReadOnlyCollection<GetProductsByIdsResponse>> result = await handler.ExecuteAsync(
            request,
            CancellationToken.None);

        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe(ProductErrors.NotFound);
    }

    [Fact]
    public async Task Execute_WithInvalidRequest_ShouldReturnValidationErrorBeforeQuery()
    {
        ProductRepositoryStub repository = new([]);
        GetProductsByIdsHandler handler = new(new GetProductsByIdsValidator(), repository);
        GetProductsByIdsRequest request = new() { ProductIds = [] };

        Result<IReadOnlyCollection<GetProductsByIdsResponse>> result = await handler.ExecuteAsync(
            request,
            CancellationToken.None);

        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNull();
        result.Error.Code.ShouldBe("VALIDATION_ERROR");
        repository.QueryWasExecuted.ShouldBeFalse();
    }

    private sealed class ProductRepositoryStub(IReadOnlyList<ProductEntity> products) : IProductRepository
    {
        public bool QueryWasExecuted { get; private set; }
        public IReadOnlyCollection<Guid>? ReceivedIds { get; private set; }

        public Task<IReadOnlyList<ProductEntity>> GetByIdsAsync(
            IReadOnlyCollection<Guid> ids,
            CancellationToken cancellationToken)
        {
            QueryWasExecuted = true;
            ReceivedIds = ids;
            return Task.FromResult(products);
        }

        public Task<bool> TryAddAsync(ProductEntity product, CancellationToken cancellationToken) =>
            Task.FromResult(false);

        public Task<ProductEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult<ProductEntity?>(null);

        public Task<Paged<ProductEntity>> ListAsync(int pageNumber, int pageSize, CancellationToken cancellationToken) =>
            Task.FromResult(new Paged<ProductEntity>([], 0, pageNumber, pageSize));
    }
}
