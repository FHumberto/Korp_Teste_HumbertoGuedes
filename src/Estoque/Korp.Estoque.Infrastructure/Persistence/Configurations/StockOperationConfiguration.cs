using Korp.Estoque.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Korp.Estoque.Infrastructure.Persistence.Configurations;

public sealed class StockOperationConfiguration : IEntityTypeConfiguration<StockOperation>
{
    public const string IdempotencyKeyUniqueIndexName = "UX_StockOperations_IdempotencyKey";

    public void Configure(EntityTypeBuilder<StockOperation> builder)
    {
        builder.ToTable("stock_operations");

        builder.HasKey(operation => operation.Id);

        builder.Property(operation => operation.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(operation => operation.IdempotencyKey)
            .HasColumnName("idempotency_key")
            .HasMaxLength(StockOperation.MaxIdempotencyKeyLength)
            .IsRequired();

        builder.HasIndex(operation => operation.IdempotencyKey)
            .IsUnique()
            .HasDatabaseName(IdempotencyKeyUniqueIndexName);

        builder.Property(operation => operation.InvoiceId)
            .HasColumnName("invoice_id")
            .IsRequired();

        builder.Property(operation => operation.PayloadHash)
            .HasColumnName("payload_hash")
            .HasMaxLength(StockOperation.MaxPayloadHashLength)
            .IsRequired();

        builder.Property(operation => operation.ProcessedAt)
            .HasColumnName("processed_at")
            .IsRequired();
    }
}
