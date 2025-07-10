namespace SharpInvoice.Modules.UserManagement.Infrastructure.Persistence.Configurations;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class InvitationConfiguration : IEntityTypeConfiguration<Invitation>
{
    public void Configure(EntityTypeBuilder<Invitation> builder)
    {
        builder.ToTable("Invitations", "management");
        builder.HasKey(i => i.Id);
        builder.HasIndex(i => i.Token).IsUnique();
        builder.HasIndex(i => i.InvitedUserEmail);
    }
}