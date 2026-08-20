using Korp.Estoque.Api;
using Korp.Estoque.Api.Extensions;
using Korp.Estoque.Application;
using Korp.Estoque.Infrastructure;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddApi(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

WebApplication app = builder.Build();

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseCorsPolicies();
app.UseRateLimiter();
app.UseAuthorization();
app.UseScalarDocumentation();

app.MapControllers();

await app.RunAsync();
