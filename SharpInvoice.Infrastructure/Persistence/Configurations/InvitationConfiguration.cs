namespace SharpInvoice.Infrastructure.Persistence.Configurations;

using SharpInvoice.Core.Domain.Entities;
using SharpInvoice.Core.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class InvitationConfiguration : IEntityTypeConfiguration<Invitation>
{
    public void Configure(EntityTypeBuilder<Invitation> builder)
    {
        builder.ToTable("Invitations");
        builder.HasKey(i => i.Id);

        // Apply filter for soft-deleted businesses
        builder.HasQueryFilter(i => !i.Business.IsDeleted);

        // Properties
        builder.Property(i => i.InvitedUserEmail)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(i => i.Token)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(i => i.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        // Indexes
        builder.HasIndex(i => i.Token).IsUnique();
        builder.HasIndex(i => i.InvitedUserEmail);
        builder.HasIndex(i => i.ExpiryDate);
        builder.HasIndex(i => i.Status);

        // Relationships
        builder.HasOne(i => i.Business)
            .WithMany(b => b.Invitations)
            .HasForeignKey(i => i.BusinessId)
            .OnDelete(DeleteBehavior.Cascade); // Delete invitations if business is deleted

        builder.HasOne(i => i.Role)
            .WithMany()
            .HasForeignKey(i => i.RoleId)
            .OnDelete(DeleteBehavior.Restrict); // Don't cascade delete invitations if role is deleted
    }
}