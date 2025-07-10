namespace SharpInvoice.Modules.UserManagement.Infrastructure.Persistence.Configurations;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles", "management");
        builder.HasKey(r => r.Id);
        builder.HasIndex(r => r.Name).IsUnique();

        // Configure the many-to-many relationship with Permission
        builder.HasMany(r => r.Permissions)
            .WithOne(rp => rp.Role)
            .HasForeignKey(rp => rp.RoleId);
    }
}