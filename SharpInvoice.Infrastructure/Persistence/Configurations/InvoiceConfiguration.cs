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

        builder.HasIndex(i => new { i.BusinessId, i.InvoiceNumber }).IsUnique();

        builder.Property(i => i.Currency).HasMaxLength(3);
        builder.Property(i => i.Status).HasConversion<string>().HasMaxLength(20);


        builder.HasMany(i => i.Items)
            .WithOne(ii => ii.Invoice)
            .HasForeignKey(ii => ii.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);


        // Relationship to Client
        builder.HasOne(i => i.Client)
            .WithMany(c => c.Invoices)
            .HasForeignKey(i => i.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship to Transactions 
        builder.HasMany(i => i.Transactions)
            .WithOne()
            .HasForeignKey(t => t.InvoiceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}