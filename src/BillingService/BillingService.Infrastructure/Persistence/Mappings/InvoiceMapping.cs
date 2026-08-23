using BillingService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BillingService.Infrastructure.Persistence.Mappings;

public sealed class InvoiceMapping : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("invoices");

        builder.HasKey(invoice => invoice.Id);

        builder.Property(invoice => invoice.Number)
            .IsRequired();

        builder.Property(invoice => invoice.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(invoice => invoice.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(invoice => invoice.RowVersion)
            .HasColumnName("xmin")
            .IsRowVersion();

        builder.HasIndex(invoice => invoice.Number)
            .IsUnique();

        builder.HasMany(invoice => invoice.Items)
            .WithOne()
            .HasForeignKey("InvoiceId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(invoice => invoice.Items)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
