namespace SharpInvoice.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SharpInvoice.Core.Domain.Entities;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("Invoices");
        builder.HasKey(i => i.Id);

        // Apply soft delete filter and also filter out invoices from soft-deleted businesses
        builder.HasQueryFilter(i => !i.IsDeleted && !i.Business.IsDeleted);

        // Properties
        builder.Property(i => i.InvoiceNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(i => i.Currency)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(i => i.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(i => i.Notes)
            .HasMaxLength(2000);

        builder.Property(i => i.Terms)
            .HasMaxLength(2000);

        // Money properties
        builder.Property(i => i.SubTotal)
            .HasPrecision(18, 2);

        builder.Property(i => i.Tax)
            .HasPrecision(18, 2);

        builder.Property(i => i.Total)
            .HasPrecision(18, 2);

        builder.Property(i => i.AmountPaid)
            .HasPrecision(18, 2);

        // Indexes
        builder.HasIndex(i => new { i.BusinessId, i.InvoiceNumber })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        builder.HasIndex(i => i.Status);
        builder.HasIndex(i => i.DueDate);
        builder.HasIndex(i => i.IssueDate);
        builder.HasIndex(i => i.ClientId);

        // Relationship to Business
        builder.HasOne(i => i.Business)
            .WithMany(b => b.Invoices)
            .HasForeignKey(i => i.BusinessId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship to Client
        builder.HasOne(i => i.Client)
            .WithMany(c => c.Invoices)
            .HasForeignKey(i => i.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship to InvoiceItems
        builder.HasMany(i => i.Items)
            .WithOne(ii => ii.Invoice)
            .HasForeignKey(ii => ii.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relationship to Transactions 
        builder.HasMany(i => i.Transactions)
            .WithOne(t => t.Invoice)
            .HasForeignKey(t => t.InvoiceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}