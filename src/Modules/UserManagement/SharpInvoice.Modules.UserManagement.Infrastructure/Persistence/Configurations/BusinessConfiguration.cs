namespace SharpInvoice.Modules.UserManagement.Infrastructure.Persistence.Configurations;

using SharpInvoice.Modules.UserManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class BusinessConfiguration : IEntityTypeConfiguration<Business>
{
    public void Configure(EntityTypeBuilder<Business> builder)
    {
        builder.ToTable("Businesses", "management");
        builder.HasKey(b => b.Id);

        // Unique constraint for Name and Country
        builder.HasIndex(b => new { b.Name, b.Country })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = 0"); // Only enforce uniqueness on non-deleted records

        builder.Property(b => b.OwnerId).IsRequired();

        // Configure as a JSON column in SQL Server
        builder.Property(b => b.ThemeSettings).IsRequired();

        // Global query filter for soft deletes
        builder.HasQueryFilter(b => !b.IsDeleted);
    }
}