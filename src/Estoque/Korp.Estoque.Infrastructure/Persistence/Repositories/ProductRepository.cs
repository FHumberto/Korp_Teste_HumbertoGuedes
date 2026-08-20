using Korp.Estoque.Application.Contracts.Persistence;
using Korp.Estoque.Domain.Abstractions.Types;
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
        return dbContext.Products.AsNoTracking()
                                 .SingleOrDefaultAsync(product => product.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken)
    {
        return await dbContext.Products.AsNoTracking()
                                       .Where(product => ids.Contains(product.Id))
                                       .ToListAsync(cancellationToken);
    }

    public async Task<Paged<Product>> ListAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        pageNumber = pageNumber < 1 ? 1 : pageNumber;
        pageSize = pageSize < 1 ? 10 : pageSize;

        IQueryable<Product> query = dbContext.Products.AsNoTracking();

        int totalRecords = await query.CountAsync(cancellationToken);

        int offset = (pageNumber - 1) * pageSize;

        if (totalRecords == 0 || offset >= totalRecords)
            return new Paged<Product>([], totalRecords, pageNumber, pageSize);

        List<Product> items = await query
            .OrderBy(product => product.Code)
            .ThenBy(product => product.Id)
            .Skip(offset)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new Paged<Product>(items, totalRecords, pageNumber, pageSize);
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
