using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Korp.Estoque.Infrastructure.Persistence.Configurations;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    #region [ CONSTANTES ]

    public const string CodeUniqueIndexName = "UX_Products_Code";
    public const string BalanceCheckConstraintName = "CK_Products_Balance_NonNegative";

    #endregion

    #region [ CONFIGURAÇÕES ]

    public void Configure(EntityTypeBuilder<Product> builder)
    {
        ConfigureConstraints(builder);
        ConfigureStructure(builder);
    }

    public static void ConfigureConstraints(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable(tableBuilder => { tableBuilder.HasCheckConstraint(BalanceCheckConstraintName, "[Balance] >= 0"); });
    }

    private static void ConfigureStructure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(product => product.Id);

        builder.Property(product => product.Id)
            .ValueGeneratedNever();

        builder.Property(product => product.Code)
            .HasMaxLength(Product.MaxCodeLength)
            .IsRequired();

        builder.HasIndex(product => product.Code)
            .IsUnique()
            .HasDatabaseName(CodeUniqueIndexName);

        builder.Property(product => product.Description)
            .HasMaxLength(Product.MaxDescriptionLength)
            .IsRequired();

        builder.Property(product => product.Balance)
            .IsRequired();

        builder.Property(product => product.CreatedAt)
            .IsRequired();

        builder.Property(product => product.UpdatedAt)
            .IsRequired(false);
    }

    #endregion
}
