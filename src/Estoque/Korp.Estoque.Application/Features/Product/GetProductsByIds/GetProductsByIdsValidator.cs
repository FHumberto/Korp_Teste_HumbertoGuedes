using FluentValidation;
using Korp.Estoque.Domain.Entities.Errors;

namespace Korp.Estoque.Application.Features.Product.GetProductsByIds;

public sealed class GetProductsByIdsValidator : AbstractValidator<GetProductsByIdsRequest>
{
    public GetProductsByIdsValidator()
    {
        RuleFor(request => request.ProductIds)
            .NotNull()
            .WithMessage("A lista de produtos é obrigatória.");

        RuleFor(request => request.ProductIds)
            .NotEmpty()
            .WithMessage("Informe ao menos um produto.")
            .When(request => request.ProductIds is not null);

        RuleFor(request => request.ProductIds)
            .Must(productIds => productIds.Distinct().Count() == productIds.Count)
            .WithMessage("A lista de produtos não pode conter identificadores repetidos.")
            .When(request => request.ProductIds is not null);

        RuleForEach(request => request.ProductIds)
            .NotEqual(Guid.Empty)
            .WithMessage(ProductErrors.IdRequired.Description)
            .When(request => request.ProductIds is not null);
    }
}
