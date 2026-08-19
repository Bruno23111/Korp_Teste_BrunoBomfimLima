using InventoryService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryService.Infrastructure.Persistence.Mappings;

public sealed class ProductMapping : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");

        builder.HasKey(product => product.Id);

        builder.Property(product => product.Code)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(product => product.Description)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(product => product.AvailableQuantity)
            .HasPrecision(19, 4)
            .IsRequired();

        builder.Property(product => product.UnitPrice)
            .HasPrecision(19, 4)
            .IsRequired();

        builder.Property(product => product.RowVersion)
            .HasColumnName("xmin")
            .IsRowVersion();

        builder.HasIndex(product => product.Code)
            .IsUnique();
    }
}
