using Korp.Estoque.Application.Contracts.UseCases;
using Korp.Estoque.Application.Features.Product.CreateProduct;
using Korp.Estoque.Application.Features.Product.GetProduct;
using Korp.Estoque.Application.Features.Product.GetProductsByIds;
using Korp.Estoque.Application.Features.Product.ListProducts;
using Korp.Estoque.Application.Features.Stock.DebitStock;
using Microsoft.Extensions.DependencyInjection;

namespace Korp.Estoque.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<CreateProductValidator>();
        services.AddUseCases();
        services.AddSingleton(TimeProvider.System);

        return services;
    }

    private static void AddUseCases(this IServiceCollection services)
    {
        services.AddScoped<ICreateProductUseCase, CreateProductHandler>();
        services.AddScoped<IGetProductUseCase, GetProductHandler>();
        services.AddScoped<IGetProductsByIdsUseCase, GetProductsByIdsHandler>();
        services.AddScoped<IListProductsUseCase, ListProductsHandler>();
        services.AddScoped<IDebitStockUseCase, DebitStockHandler>();
    }
}
