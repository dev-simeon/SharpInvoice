namespace SharpInvoice.Modules.Invoicing.Infrastructure.Persistence.Configurations;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {

        builder.ToTable("Invoices", "invoicing");
        builder.HasKey(i => i.Id);

        builder.HasIndex(i => new { i.BusinessId, i.InvoiceNumber }).IsUnique();

        builder.Property(i => i.Currency).HasMaxLength(3);
        builder.Property(i => i.Status).HasConversion<string>().HasMaxLength(20);

        // Owned types or separate tables for items. Using separate table for more flexibility.
        builder.HasMany(i => i.Items)
            .WithOne(ii => ii.Invoice)
            .HasForeignKey(ii => ii.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);


        // Relationship to Client
        builder.HasOne(i => i.Client)
            .WithMany(c => c.Invoices)
            .HasForeignKey(i => i.ClientId)
            .OnDelete(DeleteBehavior.Restrict); // Don't delete client if they have invoices

        // Relationship to Transactions (in another module)
        builder.HasMany(i => i.Transactions)
            .WithOne() // No navigation property on Transaction back to Invoice
            .HasForeignKey("InvoiceId") // Shadow property
            .OnDelete(DeleteBehavior.Cascade);
    }
}