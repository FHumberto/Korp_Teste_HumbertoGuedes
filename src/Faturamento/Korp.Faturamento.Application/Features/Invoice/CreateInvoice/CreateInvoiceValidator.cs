using Korp.Faturamento.Domain.Entities.Errors;

namespace Korp.Faturamento.Application.Features.Invoice.CreateInvoice;

public sealed class CreateInvoiceValidator : AbstractValidator<CreateInvoiceRequest>
{
    public CreateInvoiceValidator()
    {
        RuleFor(request => request.Items)
            .NotNull().WithMessage("Os itens da nota são obrigatórios.")
            .NotEmpty().WithMessage("A nota deve possuir ao menos um item.");

        RuleForEach(request => request.Items).ChildRules(item =>
        {
            item.RuleFor(value => value.ProductId)
                .NotEmpty().WithMessage(InvoiceItemErrors.ProductIdRequired.Description);
            item.RuleFor(value => value.Quantity)
                .GreaterThan(0).WithMessage(InvoiceItemErrors.InvalidQuantity.Description);
        });

        RuleFor(request => request.Items)
            .Must(items => items is null || items.Select(item => item.ProductId).Distinct().Count() == items.Count)
            .When(request => request.Items is { Count: > 0 })
            .WithMessage(InvoiceErrors.DuplicateProduct.Description);
    }
}
