using Korp.Estoque.Api.Middlewares;

namespace Korp.Estoque.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddApi(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddOpenApi();
        services.AddProblemDetails();
        services.AddExceptionHandler<ExceptionMiddleware>();

        return services;
    }
}
