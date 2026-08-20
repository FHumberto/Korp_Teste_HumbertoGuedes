using FluentValidation;

namespace Korp.Estoque.Application.Features.Product.ListProducts;

public sealed class ListProductsValidator : AbstractValidator<ListProductsRequest>
{
    public const int MaxPageSize = 100;

    public ListProductsValidator()
    {
        RuleFor(request => request.PageNumber)
            .GreaterThanOrEqualTo(1)
            .WithMessage("O número da página deve ser maior ou igual a 1.");

        RuleFor(request => request.PageSize)
            .InclusiveBetween(1, MaxPageSize)
            .WithMessage($"O tamanho da página deve estar entre 1 e {MaxPageSize}.");
    }
}
