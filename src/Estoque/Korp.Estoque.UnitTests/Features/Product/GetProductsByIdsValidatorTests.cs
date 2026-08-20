using FluentValidation.TestHelper;
using Korp.Estoque.Application.Features.Product.GetProductsByIds;

namespace Korp.Estoque.UnitTests.Features.Product;

public sealed class GetProductsByIdsValidatorTests
{
    private readonly GetProductsByIdsValidator _validator = new();

    [Fact]
    public async Task Validate_WithValidProductIds_ShouldNotHaveValidationErrors()
    {
        GetProductsByIdsRequest request = new() { ProductIds = [Guid.NewGuid(), Guid.NewGuid()] };

        TestValidationResult<GetProductsByIdsRequest> result = await _validator.TestValidateAsync(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validate_WithEmptyList_ShouldHaveValidationError()
    {
        GetProductsByIdsRequest request = new() { ProductIds = [] };

        TestValidationResult<GetProductsByIdsRequest> result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(value => value.ProductIds);
    }

    [Fact]
    public async Task Validate_WithNullList_ShouldHaveValidationError()
    {
        GetProductsByIdsRequest request = new() { ProductIds = null! };

        TestValidationResult<GetProductsByIdsRequest> result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(value => value.ProductIds);
    }

    [Fact]
    public async Task Validate_WithEmptyProductId_ShouldHaveValidationError()
    {
        GetProductsByIdsRequest request = new() { ProductIds = [Guid.Empty] };

        TestValidationResult<GetProductsByIdsRequest> result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor("ProductIds[0]");
    }

    [Fact]
    public async Task Validate_WithDuplicateProductId_ShouldHaveValidationError()
    {
        Guid productId = Guid.NewGuid();
        GetProductsByIdsRequest request = new() { ProductIds = [productId, productId] };

        TestValidationResult<GetProductsByIdsRequest> result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(value => value.ProductIds);
    }
}
