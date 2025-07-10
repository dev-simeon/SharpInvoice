namespace SharpInvoice.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SharpInvoice.Core.Domain.Entities;

public class InvoiceItemConfiguration : IEntityTypeConfiguration<InvoiceItem>
{
    public void Configure(EntityTypeBuilder<InvoiceItem> builder)
    {
        builder.ToTable("InvoiceItems");
        builder.HasKey(ii => ii.Id);

        // Apply soft delete filter and also filter out items from soft-deleted invoices or businesses
        builder.HasQueryFilter(ii => !ii.IsDeleted && !ii.Invoice.IsDeleted && !ii.Invoice.Business.IsDeleted);

        // Properties
        builder.Property(ii => ii.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(ii => ii.Unit)
            .HasMaxLength(20);

        // Money properties
        builder.Property(ii => ii.Quantity)
            .HasPrecision(18, 2);

        builder.Property(ii => ii.UnitPrice)
            .HasPrecision(18, 2);

        builder.Property(ii => ii.Total)
            .HasPrecision(18, 2);

        // Indexes
        builder.HasIndex(ii => ii.InvoiceId);

        // Relationship
        builder.HasOne(ii => ii.Invoice)
            .WithMany(i => i.Items)
            .HasForeignKey(ii => ii.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}