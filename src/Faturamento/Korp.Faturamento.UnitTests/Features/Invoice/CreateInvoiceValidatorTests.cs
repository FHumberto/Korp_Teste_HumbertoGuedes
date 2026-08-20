using FluentValidation.Results;
using Korp.Faturamento.Application.Features.Invoice.CreateInvoice;
using Shouldly;

namespace Korp.Faturamento.UnitTests.Features.Invoice;

public sealed class CreateInvoiceValidatorTests
{
    private readonly CreateInvoiceValidator _validator = new();

    [Fact]
    public async Task Validate_WhenItemsAreEmpty_ShouldFail()
    {
        var request = new CreateInvoiceRequest { Items = [] };

        ValidationResult result = await _validator.ValidateAsync(request);

        result.IsValid.ShouldBeFalse();
    }

    [Fact]
    public async Task Validate_WhenProductIsDuplicated_ShouldFail()
    {
        Guid productId = Guid.NewGuid();
        var request = new CreateInvoiceRequest
        {
            Items = [new(productId, 1), new(productId, 2)]
        };

        ValidationResult result = await _validator.ValidateAsync(request);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(error => error.ErrorMessage.Contains("já foi incluído"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Validate_WhenQuantityIsNotPositive_ShouldFail(int quantity)
    {
        var request = new CreateInvoiceRequest
        {
            Items = [new(Guid.NewGuid(), quantity)]
        };

        ValidationResult result = await _validator.ValidateAsync(request);

        result.IsValid.ShouldBeFalse();
    }
}
