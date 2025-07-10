namespace SharpInvoice.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SharpInvoice.Core.Domain.Entities;
using SharpInvoice.Core.Domain.Enums;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(u => u.Id);

        // Apply soft delete filter
        builder.HasQueryFilter(u => !u.IsDeleted);

        // Basic properties
        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false"); // Only enforce uniqueness on non-deleted users

        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.PhoneNumber)
            .HasMaxLength(20);

        builder.Property(u => u.AvatarUrl)
            .HasMaxLength(1024);

        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(1024);

        // Enum conversion
        builder.Property(u => u.ApplicationUserRole)
            .HasConversion<string>()
            .HasMaxLength(20);

        // Token properties
        builder.Property(u => u.EmailConfirmationToken)
            .HasMaxLength(128);

        builder.Property(u => u.RefreshToken)
            .HasMaxLength(512);

        builder.Property(u => u.TwoFactorCode)
            .HasMaxLength(10);

        // One-to-many relationship with ExternalLogin
        builder.HasMany(u => u.ExternalLogins)
            .WithOne(el => el.User)
            .HasForeignKey(el => el.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // One-to-many relationship with TeamMember
        builder.HasMany(u => u.TeamMembers)
            .WithOne(tm => tm.User)
            .HasForeignKey(tm => tm.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}