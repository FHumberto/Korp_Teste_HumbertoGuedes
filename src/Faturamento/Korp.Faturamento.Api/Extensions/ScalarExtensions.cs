using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Scalar.AspNetCore;

namespace Korp.Faturamento.Api.Extensions;

public static class ScalarExtensions
{
    public static IServiceCollection AddScalarDocumentation(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        })
        .AddMvc()
        .AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        })
        .AddOpenApi();

        return services;
    }

    public static WebApplication UseScalarDocumentation(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            return app;
        }

        app.DescribeApiVersions();
        app.MapOpenApi().WithDocumentPerVersion();

        IApiVersionDescriptionProvider provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

        foreach (ApiVersionDescription description in provider.ApiVersionDescriptions)
        {
            app.MapScalarApiReference($"/docs/{description.GroupName}", options =>
            {
                options.WithTitle($"Faturamento Api - {description.GroupName.ToUpper()}")
                       .WithTheme(ScalarTheme.Default)
                       .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
                       .WithOpenApiRoutePattern($"/openapi/{description.GroupName}.json")
                       .SortTagsAlphabetically()
                       .SortOperationsByMethod()
                       .ExpandAllTags()
                       .HideDeveloperTools();
            });
        }

        string? lastVersion = provider.ApiVersionDescriptions.LastOrDefault()?.GroupName;
        if (lastVersion != null)
        {
            app.MapGet("/docs", () => Results.Redirect($"/docs/{lastVersion}"))
               .ExcludeFromDescription();
        }

        return app;
    }
}
