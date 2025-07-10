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

        builder.HasOne(tm => tm.User)
            .WithMany()
            .HasForeignKey(tm => tm.UserId)
            .OnDelete(DeleteBehavior.Restrict); // Prevent user deletion if they are part of a team

        builder.HasOne(tm => tm.Business)
            .WithMany()
            .HasForeignKey(tm => tm.BusinessId)
            .OnDelete(DeleteBehavior.Cascade); // If business is deleted, remove team members

        builder.HasOne(tm => tm.Role)
            .WithMany(r => r.TeamMembers)
            .HasForeignKey(tm => tm.RoleId)
            .OnDelete(DeleteBehavior.Restrict); // Prevent role deletion if it's in use
    }
}