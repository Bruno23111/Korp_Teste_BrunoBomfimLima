using InventoryService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryService.Infrastructure.Persistence.Mappings;

public sealed class StockMovementMapping : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.ToTable("stock_movements");

        builder.HasKey(movement => movement.Id);

        builder.Property(movement => movement.OperationKey)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(movement => movement.Quantity)
            .HasPrecision(19, 4)
            .IsRequired();

        builder.Property(movement => movement.OccurredAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.HasOne<Product>()
            .WithMany()
            .HasForeignKey(movement => movement.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(movement => new { movement.ProductId, movement.OperationKey })
            .IsUnique();
    }
}
