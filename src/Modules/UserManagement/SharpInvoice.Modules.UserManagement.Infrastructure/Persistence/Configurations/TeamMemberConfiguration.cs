namespace SharpInvoice.Modules.UserManagement.Infrastructure.Persistence.Configurations;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class TeamMemberConfiguration : IEntityTypeConfiguration<TeamMember>
{
    public void Configure(EntityTypeBuilder<TeamMember> builder)
    {
        builder.ToTable("TeamMembers", "management");
        // Composite primary key
        builder.HasKey(tm => new { tm.UserId, tm.BusinessId });

        builder.HasOne(tm => tm.User)
            .WithMany() // User can be part of many businesses, but no direct navigation property on User
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