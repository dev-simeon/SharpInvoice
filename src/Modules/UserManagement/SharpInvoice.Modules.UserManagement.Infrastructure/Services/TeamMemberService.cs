namespace SharpInvoice.Modules.UserManagement.Infrastructure.Services;

using SharpInvoice.Modules.UserManagement.Application.Dtos;
using SharpInvoice.Modules.UserManagement.Application.Interfaces;
using SharpInvoice.Modules.UserManagement.Domain.Entities;
using SharpInvoice.Shared.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class TeamMemberService(
    ITeamMemberRepository teamMemberRepository,
    IInvitationRepository invitationRepository,
    IRoleRepository roleRepository,
    IPermissionRepository permissionRepository,
    IEmailSender emailSender,
    ICurrentUserProvider currentUserProvider,
    IUnitOfWork unitOfWork) : ITeamMemberService
{
    // Team Member Management
    public async Task<IEnumerable<TeamMemberDto>> GetTeamMembersAsync(Guid businessId)
    {
        var teamMembers = await teamMemberRepository.GetByBusinessIdAsync(businessId);

        return teamMembers.Select(tm => new TeamMemberDto
        {
            Id = tm.Id,
            UserId = tm.UserId,
            BusinessId = tm.BusinessId,
            Email = tm.User.Email,
            FirstName = tm.User.FirstName,
            LastName = tm.User.LastName,
            RoleName = tm.Role.Name,
            JoinedAt = tm.JoinedAt,
            IsActive = tm.IsActive
        });
    }

    public async Task InviteTeamMemberAsync(Guid businessId, string email, Guid roleId)
    {
        // Check if user is already a team member
        var existingMember = await teamMemberRepository.GetByBusinessAndEmailAsync(businessId, email);
        if (existingMember != null)
        {
            throw new InvalidOperationException("User is already a team member.");
        }

        // Check if there's already a pending invitation
        var existingInvitation = await invitationRepository.GetPendingByEmailAsync(email, businessId);
        if (existingInvitation != null)
        {
            throw new InvalidOperationException("Invitation already sent to this email.");
        }

        // Create invitation
        var invitation = Invitation.Create(email, businessId, roleId);
        await invitationRepository.AddAsync(invitation);
        await unitOfWork.SaveChangesAsync();

        // Send email invitation
        var inviteLink = $"https://yourapp.com/accept-invitation?token={invitation.Token}";
        await emailSender.SendEmailAsync(
            email,
            "Team Invitation",
            $"You've been invited to join our team. Click here to accept: {inviteLink}"
        );
    }

    public async Task AcceptInvitationAsync(string token)
    {
        var invitation = await invitationRepository.GetByTokenAsync(token);
        if (invitation == null || invitation.IsExpired)
        {
            throw new InvalidOperationException("Invalid or expired invitation.");
        }

        var userId = currentUserProvider.GetCurrentUserId();
        
        // Create team member
        var teamMember = TeamMember.Create(userId, invitation.BusinessId, invitation.RoleId);
        await teamMemberRepository.AddAsync(teamMember);

        // Mark invitation as accepted
        invitation.Accept();
        await unitOfWork.SaveChangesAsync();
    }

    public async Task RemoveTeamMemberAsync(Guid teamMemberId)
    {
        var teamMember = await teamMemberRepository.GetByIdAsync(teamMemberId);
        if (teamMember == null)
        {
            throw new InvalidOperationException("Team member not found.");
        }

        teamMember.Deactivate();
        await unitOfWork.SaveChangesAsync();
    }

    public async Task UpdateTeamMemberRoleAsync(Guid teamMemberId, Guid newRoleId)
    {
        var teamMember = await teamMemberRepository.GetByIdAsync(teamMemberId);
        if (teamMember == null)
        {
            throw new InvalidOperationException("Team member not found.");
        }

        var role = await roleRepository.GetByIdAsync(newRoleId);
        if (role == null)
        {
            throw new InvalidOperationException("Role not found.");
        }

        teamMember.UpdateRole(newRoleId);
        await unitOfWork.SaveChangesAsync();
    }

    // User Roles and Permissions
    public async Task<IEnumerable<string>> GetUserRolesAsync(Guid userId, Guid businessId)
    {
        var teamMember = await teamMemberRepository.GetByUserAndBusinessAsync(userId, businessId);
        if (teamMember == null)
        {
            return Enumerable.Empty<string>();
        }

        return new[] { teamMember.Role.Name };
    }

    public async Task<IEnumerable<string>> GetUserPermissionsAsync(Guid userId, Guid businessId)
    {
        var teamMember = await teamMemberRepository.GetByUserAndBusinessAsync(userId, businessId);
        if (teamMember == null)
        {
            return Enumerable.Empty<string>();
        }

        var permissions = await permissionRepository.GetByRoleIdAsync(teamMember.RoleId);
        return permissions.Select(p => p.Name);
    }
}