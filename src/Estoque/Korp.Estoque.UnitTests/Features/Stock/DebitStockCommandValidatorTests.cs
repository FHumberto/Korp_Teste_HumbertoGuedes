using FluentValidation.Results;
using Korp.Estoque.Application.Features.Stock.DebitStock;
using Shouldly;

namespace Korp.Estoque.UnitTests.Features.Stock;

public sealed class DebitStockCommandValidatorTests
{
    private readonly DebitStockCommandValidator _validator = new();

    [Fact]
    public async Task Validate_WithValidCommand_ShouldSucceed()
    {
        DebitStockCommand command = CreateValidCommand();

        ValidationResult result = await _validator.ValidateAsync(command);

        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public async Task Validate_WithoutIdempotencyKey_ShouldFail()
    {
        DebitStockCommand validCommand = CreateValidCommand();
        DebitStockCommand command = validCommand with { IdempotencyKey = null };

        ValidationResult result = await _validator.ValidateAsync(command);

        result.Errors.ShouldContain(error => error.PropertyName == nameof(DebitStockCommand.IdempotencyKey));
    }

    [Fact]
    public async Task Validate_WithIdempotencyKeyFromAnotherInvoice_ShouldFail()
    {
        DebitStockCommand validCommand = CreateValidCommand();
        DebitStockCommand command = validCommand with { IdempotencyKey = $"invoice:{Guid.NewGuid()}:close:v1" };

        ValidationResult result = await _validator.ValidateAsync(command);

        result.Errors.ShouldContain(error => error.PropertyName == nameof(DebitStockCommand.IdempotencyKey));
    }

    [Fact]
    public async Task Validate_WithEmptyInvoiceId_ShouldFail()
    {
        DebitStockCommand command = CreateValidCommand() with { InvoiceId = Guid.Empty };

        ValidationResult result = await _validator.ValidateAsync(command);

        result.Errors.ShouldContain(error => error.PropertyName == nameof(DebitStockCommand.InvoiceId));
    }

    [Fact]
    public async Task Validate_WithEmptyItems_ShouldFail()
    {
        DebitStockCommand command = CreateValidCommand() with { Items = [] };

        ValidationResult result = await _validator.ValidateAsync(command);

        result.Errors.ShouldContain(error => error.PropertyName == nameof(DebitStockCommand.Items));
    }

    [Fact]
    public async Task Validate_WithNullItems_ShouldFail()
    {
        DebitStockCommand command = CreateValidCommand() with { Items = null! };

        ValidationResult result = await _validator.ValidateAsync(command);

        result.Errors.ShouldContain(error => error.PropertyName == nameof(DebitStockCommand.Items));
    }

    [Fact]
    public async Task Validate_WithDuplicateProduct_ShouldFail()
    {
        Guid productId = Guid.NewGuid();
        DebitStockCommand command = CreateValidCommand() with
        {
            Items = [new DebitStockItemRequest(productId, 1), new DebitStockItemRequest(productId, 2)]
        };

        ValidationResult result = await _validator.ValidateAsync(command);

        result.Errors.ShouldContain(error => error.PropertyName == nameof(DebitStockCommand.Items));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Validate_WithNonPositiveQuantity_ShouldFail(int quantity)
    {
        DebitStockCommand command = CreateValidCommand() with
        {
            Items = [new DebitStockItemRequest(Guid.NewGuid(), quantity)]
        };

        ValidationResult result = await _validator.ValidateAsync(command);

        result.Errors.ShouldContain(error => error.PropertyName == "Items[0].Quantity");
    }

    private static DebitStockCommand CreateValidCommand()
    {
        Guid invoiceId = Guid.NewGuid();
        return new DebitStockCommand(
            $"invoice:{invoiceId}:close:v1",
            invoiceId,
            [new DebitStockItemRequest(Guid.NewGuid(), 2)]);
    }
}
