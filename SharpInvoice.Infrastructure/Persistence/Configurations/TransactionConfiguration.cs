namespace SharpInvoice.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SharpInvoice.Core.Domain.Entities;
using SharpInvoice.Core.Domain.Enums;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("Transactions");
        builder.HasKey(t => t.Id);

        // Apply soft delete filter and also filter out transactions from soft-deleted invoices or businesses
        builder.HasQueryFilter(t => !t.IsDeleted && !t.Invoice.IsDeleted && !t.Invoice.Business.IsDeleted);

        // Properties
        builder.Property(t => t.Amount)
            .HasPrecision(18, 2);

        builder.Property(t => t.PaymentMethod)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(t => t.ExternalTransactionId)
            .HasMaxLength(100);

        builder.Property(t => t.Notes)
            .HasMaxLength(1000);

        // Indexes
        builder.HasIndex(t => t.InvoiceId);
        builder.HasIndex(t => t.TransactionDate);
        builder.HasIndex(t => t.PaymentMethod);
        builder.HasIndex(t => t.ExternalTransactionId)
            .HasFilter("\"ExternalTransactionId\" IS NOT NULL");

        // Relationship - never delete transactions even if invoice is deleted
        builder.HasOne(t => t.Invoice)
            .WithMany(i => i.Transactions)
            .HasForeignKey(t => t.InvoiceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
} 