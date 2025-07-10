namespace SharpInvoice.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SharpInvoice.Core.Domain.Entities;

public class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.ToTable("Clients");
        builder.HasKey(c => c.Id);

        // Apply soft delete filter and also filter out clients from soft-deleted businesses
        builder.HasQueryFilter(c => !c.IsDeleted && !c.Business.IsDeleted);

        // Properties
        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Email)
            .HasMaxLength(256);

        builder.Property(c => c.Phone)
            .HasMaxLength(20);

        builder.Property(c => c.Address)
            .HasMaxLength(200);

        builder.Property(c => c.Country)
            .HasMaxLength(100);

        builder.Property(c => c.Locale)
            .HasMaxLength(20);

        // Unique constraint for BusinessId and Email
        builder.HasIndex(c => new { c.BusinessId, c.Email })
            .IsUnique()
            .HasFilter("\"Email\" IS NOT NULL AND \"IsDeleted\" = false"); // Only enforce uniqueness on non-deleted clients with email

        // Relationships
        builder.HasOne(c => c.Business)
            .WithMany(b => b.Clients)
            .HasForeignKey(c => c.BusinessId)
            .OnDelete(DeleteBehavior.Restrict); // Don't cascade delete clients when business is deleted

        // The Client.Delete() method already prevents deletion of clients with invoices,
        // but we also set the relationship to Restrict as an extra safety measure
        builder.HasMany(c => c.Invoices)
            .WithOne(i => i.Client)
            .HasForeignKey(i => i.ClientId)
            .OnDelete(DeleteBehavior.Restrict); // Prevent deletion of clients with invoices
    }
}