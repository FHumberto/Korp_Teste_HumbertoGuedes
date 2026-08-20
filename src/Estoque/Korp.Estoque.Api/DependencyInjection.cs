using Asp.Versioning;
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
        services.AddControllers();
        services.AddOpenApi();

        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        })
        .AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        return services;
    }
}
