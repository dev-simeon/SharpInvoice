namespace SharpInvoice.Modules.UserManagement.Infrastructure.Services;

using SharpInvoice.Modules.UserManagement.Application.Interfaces;
using SharpInvoice.Shared.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using SharpInvoice.Modules.UserManagement.Application.Dtos;
using SharpInvoice.Modules.UserManagement.Domain.Entities;
using SharpInvoice.Shared.Kernel.Exceptions;
using SharpInvoice.Modules.Auth.Application.Interfaces;
using System.Security.Cryptography;

public class TeamMemberService(AppDbContext context, IEmailSender emailSender, IHttpContextAccessor httpContextAccessor) : ITeamMemberService
{
    private Guid GetCurrentUserId()
    {
        var userId = httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(userId, out var id) ? id : throw new UnauthorizedAccessException("User is not authenticated.");
    }

    public async Task<IEnumerable<string>> GetUserRolesAsync(Guid userId, Guid businessId)
    {
        var roles = await context.TeamMembers
            .Where(tm => tm.UserId == userId && tm.BusinessId == businessId)
            .Include(tm => tm.Role)
            .Select(tm => tm.Role.Name)
            .ToListAsync();

        return roles;
    }

    public async Task InviteTeamMemberAsync(Guid businessId, string email, Guid roleId)
    {
        var business = await context.Businesses.FindAsync(businessId)
            ?? throw new NotFoundException($"Business with ID {businessId} not found.");

        var currentUserId = GetCurrentUserId();
        if (business.OwnerId != currentUserId)
            throw new ForbidException("Only the business owner can invite team members.");

        var existingUser = await context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (existingUser != null)
        {
            var isAlreadyTeamMember = await context.TeamMembers.AnyAsync(tm => tm.UserId == existingUser.Id && tm.BusinessId == businessId);
            if (isAlreadyTeamMember)
            {
                throw new BadRequestException("This user is already a member of the business.");
            }
        }

        var invitation = Invitation.Create(businessId, email, roleId, Convert.ToHexString(RandomNumberGenerator.GetBytes(16)), 24);
        context.Invitations.Add(invitation);
        await context.SaveChangesAsync();

        var invitationLink = $"https://yourapp.com/accept-invitation?token={invitation.Token}";
        var mailBody = $"You have been invited to join {business.Name}. Please click the link to accept: {invitationLink}";
        await emailSender.SendEmailAsync(email, $"Invitation to join {business.Name}", mailBody);
    }

    public async Task AcceptInvitationAsync(string token)
    {
        var invitation = await context.Invitations
            .FirstOrDefaultAsync(i => i.Token == token && i.ExpiryDate > DateTime.UtcNow)
            ?? throw new NotFoundException("Invalid or expired invitation token.");

        var currentUserId = GetCurrentUserId();
        var user = await context.Users.FindAsync(currentUserId)
            ?? throw new NotFoundException("User not found.");

        if (user.Email != invitation.InvitedUserEmail)
            throw new ForbidException("This invitation is for a different user.");

        var teamMember = TeamMember.Create(currentUserId, invitation.BusinessId, invitation.RoleId);
        context.TeamMembers.Add(teamMember);
        context.Invitations.Remove(invitation);

        await context.SaveChangesAsync();
    }

    public async Task<IEnumerable<TeamMemberDto>> GetTeamMembersForBusinessAsync(Guid businessId)
    {
        return await context.TeamMembers
            .Where(tm => tm.BusinessId == businessId)
            .Include(tm => tm.User)
            .Include(tm => tm.Role)
            .Select(tm => new TeamMemberDto(
                tm.Id,
                tm.UserId,
                tm.User.FullName,
                tm.User.Email,
                tm.RoleId,
                tm.Role.Name,
                tm.CreatedAt
            ))
            .ToListAsync();
    }

    public async Task RemoveTeamMemberAsync(Guid teamMemberId)
    {
        var teamMember = await context.TeamMembers.FindAsync(teamMemberId)
            ?? throw new NotFoundException($"Team member with ID {teamMemberId} not found.");

        var business = await context.Businesses.FindAsync(teamMember.BusinessId);
        var currentUserId = GetCurrentUserId();
        if (business.OwnerId != currentUserId)
            throw new ForbidException("Only the business owner can remove team members.");

        if (teamMember.UserId == business.OwnerId)
            throw new BadRequestException("The business owner cannot be removed.");

        context.TeamMembers.Remove(teamMember);
        await context.SaveChangesAsync();
    }

    public async Task UpdateTeamMemberRoleAsync(Guid teamMemberId, Guid newRoleId)
    {
        var teamMember = await context.TeamMembers.FindAsync(teamMemberId)
            ?? throw new NotFoundException($"Team member with ID {teamMemberId} not found.");
        
        var business = await context.Businesses.FindAsync(teamMember.BusinessId);
        var currentUserId = GetCurrentUserId();
        if (business.OwnerId != currentUserId)
            throw new ForbidException("Only the business owner can update team member roles.");

        if (teamMember.UserId == business.OwnerId)
            throw new BadRequestException("The business owner's role cannot be changed.");
            
        teamMember.UpdateRole(newRoleId);
        await context.SaveChangesAsync();
    }
}