namespace SharpInvoice.Modules.Payments.Infrastructure.Persistence.Configurations;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("Transactions", "payments");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.PaymentMethod).HasConversion<string>().HasMaxLength(20);

        // This defines the foreign key relationship to the Invoice table in another schema.
        // The relationship is owned and configured on the Invoice side.
        builder.HasIndex("InvoiceId");
    }
}