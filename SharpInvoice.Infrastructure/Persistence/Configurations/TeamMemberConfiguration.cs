namespace SharpInvoice.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SharpInvoice.Core.Domain.Entities;

public class TeamMemberConfiguration : IEntityTypeConfiguration<TeamMember>
{
    public void Configure(EntityTypeBuilder<TeamMember> builder)
    {
        builder.ToTable("TeamMembers");
        
        // Composite primary key
        builder.HasKey(tm => new { tm.UserId, tm.BusinessId });

        // Add query filter to hide team members of soft-deleted businesses
        builder.HasQueryFilter(tm => !tm.Business.IsDeleted);

        // Properties
        builder.Property(tm => tm.CreatedAt)
            .IsRequired();

        // Indexes for performance
        builder.HasIndex(tm => tm.UserId);
        builder.HasIndex(tm => tm.BusinessId);
        builder.HasIndex(tm => tm.RoleId);
        builder.HasIndex(tm => tm.CreatedAt);

        // Relationships
        builder.HasOne(tm => tm.User)
            .WithMany(u => u.TeamMembers)
            .HasForeignKey(tm => tm.UserId)
            .OnDelete(DeleteBehavior.Restrict); // Prevent user deletion if they are part of a team

        builder.HasOne(tm => tm.Business)
            .WithMany(b => b.TeamMembers)
            .HasForeignKey(tm => tm.BusinessId)
            .OnDelete(DeleteBehavior.Cascade); // If business is deleted, remove team members

        builder.HasOne(tm => tm.Role)
            .WithMany(r => r.TeamMembers)
            .HasForeignKey(tm => tm.RoleId)
            .OnDelete(DeleteBehavior.Restrict); // Prevent role deletion if it's in use
    }
}