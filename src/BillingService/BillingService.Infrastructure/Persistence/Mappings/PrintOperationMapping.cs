using BillingService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BillingService.Infrastructure.Persistence.Mappings;

public sealed class PrintOperationMapping : IEntityTypeConfiguration<PrintOperation>
{
    public void Configure(EntityTypeBuilder<PrintOperation> builder)
    {
        builder.ToTable("print_operations"); builder.HasKey(operation => operation.Id);
        builder.Property(operation => operation.IdempotencyKey).HasMaxLength(100).IsRequired();
        builder.Property(operation => operation.Status).HasConversion<int>().IsRequired();
        builder.Property(operation => operation.ErrorCode).HasMaxLength(100);
        builder.Property(operation => operation.CreatedAt).HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(operation => operation.CompletedAt).HasColumnType("timestamp with time zone");
        builder.HasIndex(operation => new { operation.InvoiceId, operation.IdempotencyKey }).IsUnique();
        builder.HasOne<Invoice>().WithMany().HasForeignKey(operation => operation.InvoiceId).OnDelete(DeleteBehavior.Restrict);
    }
}
