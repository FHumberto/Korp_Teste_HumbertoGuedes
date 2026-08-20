using Serilog;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;

namespace Korp.Estoque.Api.Extensions;

public static class SerilogExtensions
{
    public static WebApplicationBuilder AddStructuredLogging(this WebApplicationBuilder builder)
    {
        string connectionString = builder.Configuration.GetConnectionString("EstoqueDatabase")
            ?? throw new InvalidOperationException("A connection string 'EstoqueDatabase' deve ser configurada.");

        builder.Services.AddSerilog(configuration => configuration
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Service", "Estoque")
            .WriteTo.Console()
            .WriteTo.MSSqlServer(
                connectionString,
                new MSSqlServerSinkOptions
                {
                    TableName = "error_logs",
                    AutoCreateSqlDatabase = true,
                    AutoCreateSqlTable = true
                },
                restrictedToMinimumLevel: LogEventLevel.Error));

        return builder;
    }
}
