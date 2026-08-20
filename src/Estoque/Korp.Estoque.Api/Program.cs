using Korp.Estoque.Api;
using Korp.Estoque.Api.Extensions;
using Korp.Estoque.Application;
using Korp.Estoque.Infrastructure;
using Korp.Estoque.Infrastructure.Persistence;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddStructuredLogging();

builder.Services.AddApi(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

WebApplication app = builder.Build();

if (app.Configuration.GetValue<bool>("Database:ApplyMigrationsOnStartup"))
    await app.Services.ApplyInventoryMigrationsAsync();

app.UseExceptionHandler();
app.UseSerilogRequestLogging();
if (app.Configuration.GetValue("HttpsRedirection:Enabled", true))
    app.UseHttpsRedirection();
app.UseCorsPolicies();
app.UseRateLimiter();
app.UseAuthorization();
app.UseScalarDocumentation();

app.MapControllers();

await app.RunAsync();
