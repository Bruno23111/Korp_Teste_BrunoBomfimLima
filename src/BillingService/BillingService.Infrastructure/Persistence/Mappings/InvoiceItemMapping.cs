using BillingService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BillingService.Infrastructure.Persistence.Mappings;

public sealed class InvoiceItemMapping : IEntityTypeConfiguration<InvoiceItem>
{
    public void Configure(EntityTypeBuilder<InvoiceItem> builder)
    {
        builder.ToTable("invoice_items");

        builder.HasKey(item => item.Id);

        builder.Property(item => item.ProductCode)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(item => item.ProductDescription)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(item => item.Quantity)
            .HasPrecision(19, 4)
            .IsRequired();

        builder.Property<Guid>("InvoiceId")
            .IsRequired();

        builder.HasIndex("InvoiceId");
    }
}
