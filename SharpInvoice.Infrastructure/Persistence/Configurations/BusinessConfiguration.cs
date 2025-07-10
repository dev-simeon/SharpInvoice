namespace SharpInvoice.Infrastructure.Persistence.Configurations;

using SharpInvoice.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class BusinessConfiguration : IEntityTypeConfiguration<Business>
{
    public void Configure(EntityTypeBuilder<Business> builder)
    {
        builder.ToTable("Businesses");
        builder.HasKey(b => b.Id);

        // Global query filter for soft deletes - when a business is soft-deleted,
        // it will not appear in normal queries but the data remains in the database
        builder.HasQueryFilter(b => !b.IsDeleted);

        // Add index for IsActive to improve query performance
        builder.HasIndex(b => b.IsActive);
        
        // Add index for IsDeleted to improve performance of admin queries that include deleted businesses
        builder.HasIndex(b => b.IsDeleted);

        // Basic properties
        builder.Property(b => b.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(b => b.Address)
            .HasMaxLength(200);

        builder.Property(b => b.City)
            .HasMaxLength(100);

        builder.Property(b => b.State)
            .HasMaxLength(100);

        builder.Property(b => b.ZipCode)
            .HasMaxLength(20);

        builder.Property(b => b.Country)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(b => b.PhoneNumber)
            .HasMaxLength(20);

        builder.Property(b => b.Email)
            .HasMaxLength(256);

        builder.Property(b => b.Website)
            .HasMaxLength(512);

        builder.Property(b => b.LogoUrl)
            .HasMaxLength(1024);

        // Configure as a JSON column
        builder.Property(b => b.ThemeSettings)
            .IsRequired()
            .HasMaxLength(4000);

        // Unique constraint for Name and Country
        builder.HasIndex(b => new { b.Name, b.Country })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false"); // Only enforce uniqueness on non-deleted records

        // Relationships
        builder.HasOne(b => b.Owner)
            .WithMany()
            .HasForeignKey(b => b.OwnerId)
            .OnDelete(DeleteBehavior.Restrict); // Don't cascade delete businesses when owner is deleted

        // When a business is soft-deleted, team members remain in the database but are filtered out
        // by the business's query filter. They can be restored when the business is restored.
        builder.HasMany(b => b.TeamMembers)
            .WithOne(tm => tm.Business)
            .HasForeignKey(tm => tm.BusinessId);

        // Clients are preserved when a business is soft-deleted
        builder.HasMany(b => b.Clients)
            .WithOne(c => c.Business)
            .HasForeignKey(c => c.BusinessId);

        // Invoices are preserved when a business is soft-deleted
        builder.HasMany(b => b.Invoices)
            .WithOne(i => i.Business)
            .HasForeignKey(i => i.BusinessId);

        // Invitations are preserved when a business is soft-deleted
        builder.HasMany(b => b.Invitations)
            .WithOne(i => i.Business)
            .HasForeignKey(i => i.BusinessId);
    }
}