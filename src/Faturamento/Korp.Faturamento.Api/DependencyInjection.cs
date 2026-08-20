using Korp.Faturamento.Api.Extensions;
using Korp.Faturamento.Api.Middlewares;

namespace Korp.Faturamento.Api;

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
