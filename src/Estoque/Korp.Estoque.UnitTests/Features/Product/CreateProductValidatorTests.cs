using FluentValidation.Results;
using Korp.Estoque.Application.Features.Product.CreateProduct;
using ProductEntity = Korp.Estoque.Domain.Entities.Product;

namespace Korp.Estoque.UnitTests.Features.Product;

public sealed class CreateProductValidatorTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Validate_WhenCodeIsMissing_ShouldFail(string? code)
    {
        CreateProductRequest request = CreateValidRequest();
        request.Code = code!;

        ValidationResult result = await new CreateProductValidator().ValidateAsync(request);

        result.Errors.ShouldContain(error => error.PropertyName == nameof(CreateProductRequest.Code));
    }

    [Fact]
    public async Task Validate_WhenCodeExceedsMaximumLength_ShouldFail()
    {
        CreateProductRequest request = CreateValidRequest();
        request.Code = new string('A', ProductEntity.MaxCodeLength + 1);

        ValidationResult result = await new CreateProductValidator().ValidateAsync(request);

        result.Errors.ShouldContain(error => error.PropertyName == nameof(CreateProductRequest.Code));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Validate_WhenDescriptionIsMissing_ShouldFail(string? description)
    {
        CreateProductRequest request = CreateValidRequest();
        request.Description = description!;

        ValidationResult result = await new CreateProductValidator().ValidateAsync(request);

        result.Errors.ShouldContain(error => error.PropertyName == nameof(CreateProductRequest.Description));
    }

    [Fact]
    public async Task Validate_WhenDescriptionExceedsMaximumLength_ShouldFail()
    {
        CreateProductRequest request = CreateValidRequest();
        request.Description = new string('A', ProductEntity.MaxDescriptionLength + 1);

        ValidationResult result = await new CreateProductValidator().ValidateAsync(request);

        result.Errors.ShouldContain(error => error.PropertyName == nameof(CreateProductRequest.Description));
    }

    [Fact]
    public async Task Validate_WhenInitialBalanceIsNegative_ShouldFail()
    {
        CreateProductRequest request = CreateValidRequest();
        request.InitialBalance = -1;

        ValidationResult result = await new CreateProductValidator().ValidateAsync(request);

        result.Errors.ShouldContain(error => error.PropertyName == nameof(CreateProductRequest.InitialBalance));
    }

    private static CreateProductRequest CreateValidRequest() => new()
    {
        Code = "PROD-001",
        Description = "Produto de demonstração",
        InitialBalance = 10
    };
}
