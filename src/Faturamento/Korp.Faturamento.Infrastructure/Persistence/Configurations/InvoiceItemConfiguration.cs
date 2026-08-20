using Korp.Faturamento.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Korp.Faturamento.Infrastructure.Persistence.Configurations;

public sealed class InvoiceItemConfiguration : IEntityTypeConfiguration<InvoiceItem>
{
    public void Configure(EntityTypeBuilder<InvoiceItem> builder)
    {
        builder.ToTable("invoice_items");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property<Guid>("invoice_id").HasColumnName("invoice_id").IsRequired();
        builder.Property(item => item.ProductId).HasColumnName("product_id").IsRequired();
        builder.Property(item => item.ProductCode).HasColumnName("product_code").HasMaxLength(InvoiceItem.MaxProductCodeLength).IsRequired();
        builder.Property(item => item.ProductDescription).HasColumnName("product_description").HasMaxLength(InvoiceItem.MaxProductDescriptionLength).IsRequired();
        builder.Property(item => item.Quantity).HasColumnName("quantity").IsRequired();
        builder.HasIndex("invoice_id", nameof(InvoiceItem.ProductId))
            .IsUnique()
            .HasDatabaseName("UX_InvoiceItems_InvoiceId_ProductId");
        builder.ToTable(table => table.HasCheckConstraint("CK_InvoiceItems_Quantity_Positive", "[quantity] > 0"));
    }
}
