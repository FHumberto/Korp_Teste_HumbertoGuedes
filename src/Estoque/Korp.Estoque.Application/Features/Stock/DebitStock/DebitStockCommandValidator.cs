using Korp.Estoque.Domain.Entities;
using Korp.Estoque.Domain.Entities.Errors;

namespace Korp.Estoque.Application.Features.Stock.DebitStock;

public sealed class DebitStockCommandValidator : AbstractValidator<DebitStockCommand>
{
    public DebitStockCommandValidator()
    {
        RuleFor(command => command.IdempotencyKey)
            .NotEmpty().WithMessage(StockOperationErrors.IdempotencyKeyRequired.Description)
            .MaximumLength(StockOperation.MaxIdempotencyKeyLength).WithMessage(StockOperationErrors.IdempotencyKeyTooLong.Description)
            .Must((command, idempotencyKey) => IsExpectedIdempotencyKey(command.InvoiceId, idempotencyKey))
            .WithMessage("O header Idempotency-Key não corresponde à nota informada.");

        RuleFor(command => command.InvoiceId)
            .NotEmpty().WithMessage(StockOperationErrors.InvoiceIdRequired.Description);

        RuleFor(command => command.Items)
            .NotNull().WithMessage("A lista de itens é obrigatória.");

        RuleFor(command => command.Items)
            .NotEmpty().WithMessage("Informe ao menos um item para a baixa.")
            .When(command => command.Items is not null);

        RuleFor(command => command.Items)
            .Must(items => items.Select(item => item.ProductId).Distinct().Count() == items.Count)
            .WithMessage("A lista de itens não pode conter produtos repetidos.")
            .When(command => command.Items is not null);

        RuleForEach(command => command.Items).ChildRules(item =>
        {
            item.RuleFor(value => value.ProductId)
                .NotEmpty()
                .WithMessage(ProductErrors.IdRequired.Description);

            item.RuleFor(value => value.Quantity)
                .GreaterThan(0)
                .WithMessage(ProductErrors.InvalidDebitQuantity.Description);
        })
        .When(command => command.Items is not null);
    }

    private static bool IsExpectedIdempotencyKey(Guid invoiceId, string? idempotencyKey)
    {
        if (invoiceId == Guid.Empty || string.IsNullOrWhiteSpace(idempotencyKey))
            return true;

        return string.Equals(idempotencyKey, $"invoice:{invoiceId}:close:v1", StringComparison.Ordinal);
    }
}
