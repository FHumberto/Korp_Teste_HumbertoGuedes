using Korp.Estoque.Api.Extensions;
using Korp.Estoque.Api.Middlewares;

namespace Korp.Estoque.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddProblemDetails();
        services.AddExceptionHandler<ExceptionMiddleware>();
        services.AddCorsPolicies(configuration);
        services.AddRateLimiterPolicies(configuration);
        services.AddScalarDocumentation();
        services.AddControllers();

        return services;
    }
}
