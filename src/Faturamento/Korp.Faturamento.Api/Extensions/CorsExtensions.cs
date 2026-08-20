namespace Korp.Faturamento.Api.Extensions;

public static class CorsExtensions
{
    #region [ CONSTANTES ]

    public const string POLICY_DEVELOPMENT = "CORS_Development";
    public const string POLICY_PRODUCTION = "CORS_Production";

    #endregion

    public static IServiceCollection AddCorsPolicies(this IServiceCollection services, IConfiguration configuration)
    {
        string[] allowedOrigins = configuration.GetSection("CORS:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

        services.AddCors(options =>
        {
            options.AddPolicy(POLICY_DEVELOPMENT, builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            });

            options.AddPolicy(POLICY_PRODUCTION, builder =>
            {
                if (allowedOrigins.Length > 0)
                {
                    builder.WithOrigins(allowedOrigins)
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                }
            });
        });

        return services;
    }

    public static WebApplication UseCorsPolicies(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseCors(POLICY_DEVELOPMENT);
        }
        else
        {
            app.UseCors(POLICY_PRODUCTION);
        }

        return app;
    }
}
