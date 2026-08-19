using FluentValidation;
using Korp.Estoque.Domain.Entities.Errors;
using ProductEntity = Korp.Estoque.Domain.Entities.Product;

namespace Korp.Estoque.Application.Features.Product.CreateProduct;

public sealed class CreateProductValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductValidator()
    {
        RuleFor(request => request.Code)
            .NotEmpty().WithMessage(ProductErrors.CodeRequired.Description)
            .MaximumLength(ProductEntity.MaxCodeLength).WithMessage($"O código do produto deve possuir no máximo {ProductEntity.MaxCodeLength} caracteres.");

        RuleFor(request => request.Description)
            .NotEmpty().WithMessage(ProductErrors.DescriptionRequired.Description)
            .MaximumLength(ProductEntity.MaxDescriptionLength).WithMessage($"A descrição do produto deve possuir no máximo {ProductEntity.MaxDescriptionLength} caracteres.");

        RuleFor(request => request.InitialBalance)
            .GreaterThanOrEqualTo(0)
            .WithMessage(ProductErrors.NegativeBalance.Description);
    }
}
