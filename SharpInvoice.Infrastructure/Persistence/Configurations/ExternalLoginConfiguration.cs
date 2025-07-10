namespace SharpInvoice.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SharpInvoice.Core.Domain.Entities;

public class ExternalLoginConfiguration : IEntityTypeConfiguration<ExternalLogin>
{
    public void Configure(EntityTypeBuilder<ExternalLogin> builder)
    {
        builder.ToTable("ExternalLogins");

        // Composite primary key
        builder.HasKey(el => new { el.LoginProvider, el.ProviderKey });

        // Relationship to User
        builder.HasOne(el => el.User)
            .WithMany(u => u.ExternalLogins)
            .HasForeignKey(el => el.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
} 