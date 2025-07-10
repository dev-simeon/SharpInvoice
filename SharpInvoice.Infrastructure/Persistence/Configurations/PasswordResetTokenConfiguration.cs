namespace SharpInvoice.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SharpInvoice.Core.Domain.Entities;

public class PasswordResetTokenConfiguration : IEntityTypeConfiguration<PasswordResetToken>
{
    public void Configure(EntityTypeBuilder<PasswordResetToken> builder)
    {
        builder.ToTable("PasswordResetTokens");
        builder.HasKey(t => t.Id);

        // Properties
        builder.Property(t => t.Token)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(t => t.UserEmail)
            .IsRequired()
            .HasMaxLength(256);

        // Indexes
        builder.HasIndex(t => t.Token).IsUnique();
        builder.HasIndex(t => t.UserEmail);
        builder.HasIndex(t => t.ExpiryDate);
        builder.HasIndex(t => t.IsUsed);

        // Filter out expired or used tokens by default
        builder.HasQueryFilter(t => !t.IsUsed && t.ExpiryDate > DateTime.UtcNow);
    }
}