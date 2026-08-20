using Korp.Estoque.Application.Abstractions.Wrappers;
using Korp.Estoque.Application.Contracts.Persistence;
using Korp.Estoque.Application.Features.Product.CreateProduct;
using Korp.Estoque.Domain.Entities.Errors;
using Korp.Estoque.Domain.Abstractions.Types;
using Shouldly;
using ProductEntity = Korp.Estoque.Domain.Entities.Product;

namespace Korp.Estoque.UnitTests.Features.Product;

public sealed class CreateProductHandlerTests
{
    [Fact]
    public async Task Execute_WithValidRequest_ShouldPersistAndReturnCreatedProduct()
    {
        ProductRepositoryStub repository = new();
        CreateProductHandler handler = CreateHandler(repository);
        CreateProductRequest request = new()
        {
            Code = " prod-001 ",
            Description = " Produto de demonstração ",
            InitialBalance = 10
        };

        DateTimeOffset utcBeforeExecution = DateTimeOffset.UtcNow;
        Result<CreateProductResponse> result = await handler.ExecuteAsync(request, CancellationToken.None);
        DateTimeOffset utcAfterExecution = DateTimeOffset.UtcNow;

        result.IsSuccess.ShouldBeTrue();
        repository.SavedProduct.ShouldNotBeNull();
        result.Value.Id.ShouldBe(repository.SavedProduct.Id);
        result.Value.Code.ShouldBe("PROD-001");
        result.Value.Description.ShouldBe("Produto de demonstração");
        result.Value.Balance.ShouldBe(request.InitialBalance);
        result.Value.CreatedAt.ShouldBeGreaterThanOrEqualTo(utcBeforeExecution);
        result.Value.CreatedAt.ShouldBeLessThanOrEqualTo(utcAfterExecution);
    }

    [Fact]
    public async Task Execute_WhenCodeAlreadyExists_ShouldReturnConflict()
    {
        ProductRepositoryStub repository = new(productCreated: false);
        CreateProductHandler handler = CreateHandler(repository);
        CreateProductRequest request = new()
        {
            Code = "PROD-001",
            Description = "Produto de demonstração",
            InitialBalance = 10
        };

        Result<CreateProductResponse> result = await handler.ExecuteAsync(request, CancellationToken.None);

        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe(ProductErrors.CodeAlreadyExists);
    }

    [Fact]
    public async Task Execute_WhenRequestIsInvalid_ShouldReturnValidationErrorBeforePersistence()
    {
        ProductRepositoryStub repository = new();
        CreateProductHandler handler = CreateHandler(repository);
        CreateProductRequest request = new()
        {
            Code = " ",
            Description = "Produto de demonstração",
            InitialBalance = 10
        };

        Result<CreateProductResponse> result = await handler.ExecuteAsync(request, CancellationToken.None);

        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNull();
        result.Error.Code.ShouldBe("VALIDATION_ERROR");
        IReadOnlyDictionary<string, string[]> validationDetails = result.Error.ValidationDetails.ShouldNotBeNull();
        validationDetails.ShouldContainKey(nameof(CreateProductRequest.Code));
        repository.SavedProduct.ShouldBeNull();
    }

    private static CreateProductHandler CreateHandler(IProductRepository repository) =>
        new(new CreateProductValidator(), repository);

    private sealed class ProductRepositoryStub(bool productCreated = true) : IProductRepository
    {
        public ProductEntity? SavedProduct { get; private set; }

        public Task<bool> TryAddAsync(ProductEntity product, CancellationToken cancellationToken)
        {
            SavedProduct = product;
            return Task.FromResult(productCreated);
        }

        public Task<ProductEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult<ProductEntity?>(null);

        public Task<IReadOnlyList<ProductEntity>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<ProductEntity>>([]);

        public Task<Paged<ProductEntity>> ListAsync(int pageNumber, int pageSize, CancellationToken cancellationToken) =>
            Task.FromResult(new Paged<ProductEntity>([], 0, pageNumber, pageSize));
    }
}
