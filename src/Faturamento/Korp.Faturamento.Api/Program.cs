using Korp.Faturamento.Api;
using Korp.Faturamento.Api.Extensions;
using Korp.Faturamento.Application;
using Korp.Faturamento.Infrastructure;
using Korp.Faturamento.Infrastructure.Persistence;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddApi(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

WebApplication app = builder.Build();

if (app.Configuration.GetValue<bool>("Database:ApplyMigrationsOnStartup"))
    await app.Services.ApplyBillingMigrationsAsync();

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseCorsPolicies();
app.UseRateLimiter();
app.UseAuthorization();
app.UseScalarDocumentation();

app.MapControllers();

await app.RunAsync();
