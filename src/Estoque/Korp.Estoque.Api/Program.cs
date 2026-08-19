using Korp.Estoque.Api;
using Korp.Estoque.Application;
using Korp.Estoque.Infrastructure;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddApi();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// TODO: Aplicar as migrations do Estoque na etapa de provisionamento do ambiente antes de iniciar a API.
await app.RunAsync();
