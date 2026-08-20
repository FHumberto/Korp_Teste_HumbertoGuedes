using Serilog;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;

namespace Korp.Faturamento.Api.Extensions;

public static class SerilogExtensions
{
    public static WebApplicationBuilder AddStructuredLogging(this WebApplicationBuilder builder)
    {
        string connectionString = builder.Configuration.GetConnectionString("FaturamentoDatabase")
            ?? throw new InvalidOperationException("A connection string 'FaturamentoDatabase' deve ser configurada.");

        builder.Services.AddSerilog(configuration => configuration
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Service", "Faturamento")
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
