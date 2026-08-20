using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Storage;

namespace Korp.Estoque.Infrastructure.Persistence.Repositories;

public sealed class StockDebitRepository(InventoryDbContext dbContext) : IStockDebitRepository
{
    #region [ CONSTANTES ]

    private const int CannotInsertDuplicateKeyRowErrorNumber = 2601;
    private const int PrimaryKeyOrUniqueConstraintViolationErrorNumber = 2627;

    #endregion

    #region [ ESCRITA ]

    public async Task<StockDebitPersistenceResult> DebitAsync(StockDebitPersistenceCommand command, CancellationToken cancelationToken)
    {
        IDbContextTransaction? transaction = await dbContext.Database.BeginTransactionAsync(cancelationToken);

        try
        {
            StockOperation? existingOperation = await FindOperationAsync(command.IdempotencyKey, cancelationToken);

            if (existingOperation is not null)
                return FromExistingOperation(existingOperation, command);

            StockOperation operation = StockOperation.Create(command.OperationId, command.IdempotencyKey, command.InvoiceId, command.PayloadHash, command.ProcessedAt);

            dbContext.StockOperations.Add(operation);

            try
            {
                await dbContext.SaveChangesAsync(cancelationToken);
            }
            catch (DbUpdateException exception) when (IsUniqueConstraintViolation(exception))
            {
                await transaction.RollbackAsync(cancelationToken);
                await transaction.DisposeAsync();
                transaction = null;
                dbContext.Entry(operation).State = EntityState.Detached;

                StockOperation concurrentOperation = await FindOperationAsync(command.IdempotencyKey, cancelationToken)
                    ?? throw new InvalidOperationException("A operação idempotente concorrente não foi encontrada após a violação de unicidade.");

                return FromExistingOperation(concurrentOperation, command);
            }

            foreach (StockDebitPersistenceItem item in command.Items.OrderBy(item => item.ProductId))
            {
                int affectedRows = await dbContext.Products
                    .Where(product => product.Id == item.ProductId && product.Balance >= item.Quantity)
                    .ExecuteUpdateAsync(setters => setters.SetProperty(product => product.Balance, product => product.Balance - item.Quantity).SetProperty(product => product.UpdatedAt, command.ProcessedAt),
                        cancelationToken);

                if (affectedRows == 1)
                    continue;

                bool productExists = await dbContext.Products
                    .AsNoTracking()
                    .AnyAsync(product => product.Id == item.ProductId, cancelationToken);

                await transaction.RollbackAsync(cancelationToken);

                return new StockDebitPersistenceResult
                (
                    productExists ? StockDebitPersistenceStatus.InsufficientStock : StockDebitPersistenceStatus.ProductNotFound,
                    command.OperationId,
                    command.InvoiceId,
                    command.ProcessedAt
                );
            }

            await transaction.CommitAsync(cancelationToken);

            return new StockDebitPersistenceResult(StockDebitPersistenceStatus.Succeeded, operation.Id, operation.InvoiceId, operation.ProcessedAt);
        }
        finally
        {
            if (transaction is not null)
                await transaction.DisposeAsync();
        }
    }

    #endregion

    #region [ LEITURA ]

    private Task<StockOperation?> FindOperationAsync(string idempotencyKey, CancellationToken cancellationToken) => dbContext.StockOperations.AsNoTracking()
                                        .SingleOrDefaultAsync(operation => operation.IdempotencyKey == idempotencyKey, cancellationToken);

    private static StockDebitPersistenceResult FromExistingOperation(StockOperation operation, StockDebitPersistenceCommand command)
    {
        StockDebitPersistenceStatus status =
            operation.InvoiceId == command.InvoiceId && operation.PayloadHash == command.PayloadHash
                ? StockDebitPersistenceStatus.AlreadyProcessed
                : StockDebitPersistenceStatus.IdempotencyConflict;

        return new StockDebitPersistenceResult(
            status,
            operation.Id,
            operation.InvoiceId,
            operation.ProcessedAt);
    }

    #endregion

    #region [ AUXILIAR ]

    private static bool IsUniqueConstraintViolation(DbUpdateException exception) => exception.GetBaseException() is SqlException
    {
        Number: CannotInsertDuplicateKeyRowErrorNumber or PrimaryKeyOrUniqueConstraintViolationErrorNumber
    };

    #endregion
}
