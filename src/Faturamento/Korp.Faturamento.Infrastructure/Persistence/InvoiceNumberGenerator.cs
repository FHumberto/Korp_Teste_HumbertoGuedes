using System.Data;
using System.Data.Common;
using Korp.Faturamento.Application.Contracts.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Korp.Faturamento.Infrastructure.Persistence;

public sealed class InvoiceNumberGenerator(BillingDbContext dbContext) : IInvoiceNumberGenerator
{
    public const string SequenceName = "invoice_number_sequence";

    public async Task<long> GetNextAsync(CancellationToken cancellationToken)
    {
        DbConnection connection = dbContext.Database.GetDbConnection();
        bool shouldClose = connection.State != ConnectionState.Open;

        if (shouldClose)
            await connection.OpenAsync(cancellationToken);

        try
        {
            await using DbCommand command = connection.CreateCommand();
            command.CommandText = $"SELECT NEXT VALUE FOR [{SequenceName}]";
            object? value = await command.ExecuteScalarAsync(cancellationToken);
            return Convert.ToInt64(value);
        }
        finally
        {
            if (shouldClose)
                await connection.CloseAsync();
        }
    }
}
