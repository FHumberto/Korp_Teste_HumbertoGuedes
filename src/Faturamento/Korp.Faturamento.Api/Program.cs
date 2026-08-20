using Korp.Faturamento.Api;
using Korp.Faturamento.Api.Extensions;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddApi(builder.Configuration);

WebApplication app = builder.Build();

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseCorsPolicies();
app.UseRateLimiter();
app.UseAuthorization();
app.UseScalarDocumentation();

app.MapControllers();

await app.RunAsync();
