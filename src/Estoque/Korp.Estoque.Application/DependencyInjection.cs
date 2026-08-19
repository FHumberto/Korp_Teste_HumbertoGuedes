using FluentValidation;
using Korp.Estoque.Application.Contracts.UseCases;
using Korp.Estoque.Application.Features.Product.CreateProduct;
using Korp.Estoque.Application.Features.Product.GetProduct;
using Microsoft.Extensions.DependencyInjection;

namespace Korp.Estoque.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<CreateProductValidator>();
        services.AddScoped<ICreateProductUseCase, CreateProductHandler>();
        services.AddScoped<IGetProductUseCase, GetProductHandler>();

        return services;
    }
}
