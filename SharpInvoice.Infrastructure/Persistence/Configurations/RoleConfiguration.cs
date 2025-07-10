namespace SharpInvoice.Infrastructure.Persistence.Configurations;

using SharpInvoice.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");
        builder.HasKey(r => r.Id);

        // Apply soft delete filter
        builder.HasQueryFilter(r => !r.IsDeleted);

        // Properties
        builder.Property(r => r.Name)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(r => r.Description)
            .HasMaxLength(500);

        // A role name should be unique
        builder.HasIndex(r => r.Name)
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        // Relationships
        builder.HasMany(r => r.TeamMembers)
            .WithOne(tm => tm.Role)
            .HasForeignKey(tm => tm.RoleId);
    }
}