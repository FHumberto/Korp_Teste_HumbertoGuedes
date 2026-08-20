using System.Diagnostics.CodeAnalysis;
using System.Threading.RateLimiting;
using Korp.Estoque.Api.Settings;

namespace Korp.Estoque.Api.Extensions;

[ExcludeFromCodeCoverage]
public static class RateLimiterExtensions
{
    public static IServiceCollection AddRateLimiterPolicies(this IServiceCollection services, IConfiguration configuration)
    {
        RateLimiterSettings? settings = configuration.GetSection("RateLimiting")
                                                     .Get<RateLimiterSettings>() ?? new RateLimiterSettings();

        services.AddRateLimiter(options =>
        {
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            {
                string? clientIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown-ip";

                return RateLimitPartition.GetFixedWindowLimiter(clientIp, _ =>
                    new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = settings.PermitLimit,
                        Window = TimeSpan.FromMinutes(settings.WindowInMinutes),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = settings.QueueLimit
                    });
            });

            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await context.HttpContext.Response.WriteAsJsonAsync
                (
                    new { message = "Você excedeu o limite de requisições. Tente novamente mais tarde." },
                    cancellationToken
                );
            };
        });

        return services;
    }
}
