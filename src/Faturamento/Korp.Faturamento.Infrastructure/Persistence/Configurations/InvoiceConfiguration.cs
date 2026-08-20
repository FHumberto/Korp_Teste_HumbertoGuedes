using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Korp.Faturamento.Infrastructure.Persistence.Configurations;

public sealed class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("invoices");
        builder.HasKey(invoice => invoice.Id);
        builder.Property(invoice => invoice.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(invoice => invoice.Number).HasColumnName("number").IsRequired();
        builder.HasIndex(invoice => invoice.Number).IsUnique().HasDatabaseName("UX_Invoices_Number");
        builder.Property(invoice => invoice.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(invoice => invoice.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(invoice => invoice.ClosedAt).HasColumnName("closed_at");
        builder.HasMany(invoice => invoice.Items)
            .WithOne()
            .HasForeignKey("invoice_id")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata.FindNavigation(nameof(Invoice.Items))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

    }
}
