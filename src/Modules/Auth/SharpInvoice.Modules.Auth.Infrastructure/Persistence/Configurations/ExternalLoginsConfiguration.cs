namespace SharpInvoice.Modules.Auth.Infrastructure.Persistence.Configurations;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ExternalLoginConfiguration : IEntityTypeConfiguration<ExternalLogin>
{
    public void Configure(EntityTypeBuilder<ExternalLogin> builder)
    {
        builder.ToTable("ExternalLogins", "auth");

        builder.HasKey(el => el.Id);

        // Composite index to ensure a user can only link a specific provider once.
        builder.HasIndex(el => new { el.LoginProvider, el.ProviderKey }).IsUnique();
    }
}