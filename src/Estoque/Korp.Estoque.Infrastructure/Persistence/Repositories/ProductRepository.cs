using Korp.Estoque.Application.Contracts.Persistence;
using Korp.Estoque.Domain.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Korp.Estoque.Infrastructure.Persistence.Repositories;

public sealed class ProductRepository(InventoryDbContext dbContext) : IProductRepository
{
    #region [ CONSTANTES ]

    private const int CannotInsertDuplicateKeyRowErrorNumber = 2601;
    private const int PrimaryKeyOrUniqueConstraintViolationErrorNumber = 2627;

    #endregion

    #region [ LEITURA ]

    public Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return dbContext.Products
            .AsNoTracking()
            .SingleOrDefaultAsync(product => product.Id == id, cancellationToken);
    }

    #endregion

    #region [ ESCRITA ]

    public async Task<bool> TryAddAsync(Product product, CancellationToken cancellationToken)
    {
        dbContext.Products.Add(product);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException exception) when (IsUniqueConstraintViolation(exception))
        {
            dbContext.Entry(product).State = EntityState.Detached;
            return false;
        }
    }

    #endregion

    #region [ AUXILIARES ]

    private static bool IsUniqueConstraintViolation(DbUpdateException exception)
    {
        return exception.GetBaseException() is SqlException
        {
            Number: CannotInsertDuplicateKeyRowErrorNumber or PrimaryKeyOrUniqueConstraintViolationErrorNumber
        };
    }

    #endregion
}
