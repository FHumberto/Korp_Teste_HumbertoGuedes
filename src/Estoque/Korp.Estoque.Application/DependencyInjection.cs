using FluentValidation;
using Korp.Estoque.Application.Features.Product.CreateProduct;
using Microsoft.Extensions.DependencyInjection;

namespace Korp.Estoque.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<CreateProductValidator>();

        return services;
    }
}
