using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Korp.Estoque.Infrastructure.Persistence.Configurations;

public sealed class StockOperationConfiguration : IEntityTypeConfiguration<StockOperation>
{
    #region [ CONSTANTES ]

    public const string IdempotencyKeyUniqueIndexName = "UX_StockOperations_IdempotencyKey";

    #endregion

    #region [ CONFIGURAÇÕES ]

    public void Configure(EntityTypeBuilder<StockOperation> builder)
    {
        builder.HasKey(operation => operation.Id);

        builder.Property(operation => operation.Id)
            .ValueGeneratedNever();

        builder.Property(operation => operation.IdempotencyKey)
            .HasMaxLength(StockOperation.MaxIdempotencyKeyLength)
            .IsRequired();

        builder.HasIndex(operation => operation.IdempotencyKey)
            .IsUnique()
            .HasDatabaseName(IdempotencyKeyUniqueIndexName);

        builder.Property(operation => operation.InvoiceId)
            .IsRequired();

        builder.Property(operation => operation.PayloadHash)
            .HasMaxLength(StockOperation.MaxPayloadHashLength)
            .IsRequired();

        builder.Property(operation => operation.ProcessedAt)
            .IsRequired();
    }

    #endregion
}
