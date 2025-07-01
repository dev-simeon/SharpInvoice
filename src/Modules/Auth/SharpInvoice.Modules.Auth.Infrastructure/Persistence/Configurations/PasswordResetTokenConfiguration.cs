namespace SharpInvoice.Modules.Auth.Infrastructure.Persistence.Configurations;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class PasswordResetTokenConfiguration : IEntityTypeConfiguration<PasswordResetToken>
{
    public void Configure(EntityTypeBuilder<PasswordResetToken> builder)
    {
        builder.ToTable("PasswordResetTokens", "auth");

        builder.HasKey(t => t.Id);

        builder.HasIndex(t => t.Token).IsUnique();

        builder.Property(t => t.UserEmail).IsRequired();
    }
}