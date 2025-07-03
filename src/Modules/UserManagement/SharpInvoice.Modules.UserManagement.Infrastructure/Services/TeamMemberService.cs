namespace SharpInvoice.Modules.UserManagement.Infrastructure.Services;

using SharpInvoice.Modules.UserManagement.Application.Interfaces;
using SharpInvoice.Shared.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SharpInvoice.Modules.UserManagement.Application.Dtos;
using SharpInvoice.Modules.UserManagement.Domain.Entities;
using SharpInvoice.Shared.Kernel.Exceptions;
using SharpInvoice.Modules.Auth.Application.Interfaces;
using System.Security.Cryptography;
using SharpInvoice.Shared.Infrastructure.Interfaces;

public class TeamMemberService(AppDbContext context, IEmailSender emailSender, ICurrentUserProvider currentUserProvider) : ITeamMemberService
{
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

        var currentUserId = currentUserProvider.GetCurrentUserId();
        if (business.OwnerId != currentUserId)
            throw new ForbidException("Only the business owner can invite team members.");

        var invitedUser = await context.Users.FirstOrDefaultAsync(u => u.Email == email);
        
        if (invitedUser != null)
        {
            // Check if user is already a team member
            var existingMember = await context.TeamMembers
                .FirstOrDefaultAsync(tm => tm.UserId == invitedUser.Id && tm.BusinessId == businessId);
            
            if (existingMember != null)
                throw new BadRequestException("This user is already a team member.");

            // Add user directly if they already exist
            var teamMember = TeamMember.Create(invitedUser.Id, businessId, roleId);
            context.TeamMembers.Add(teamMember);
            await context.SaveChangesAsync();

            // Send notification email
            await SendTeamMemberAddedEmailAsync(email, business.Name);
        }
        else
        {
            // Create an invitation if user doesn't exist
            var invitationToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            var invitation = Invitation.Create(businessId, email, roleId, invitationToken, 168); // 7 days expiry
            context.Invitations.Add(invitation);
            await context.SaveChangesAsync();

            // Send invitation email
            await SendInvitationEmailAsync(email, business.Name, invitationToken);
        }
    }

    public async Task AcceptInvitationAsync(string token)
    {
        var invitation = await context.Invitations
            .Include(i => i.Business)
            .FirstOrDefaultAsync(i => i.Token == token && i.Status == InvitationStatus.Pending && i.ExpiryDate > DateTime.UtcNow)
            ?? throw new NotFoundException("Invitation not found or has expired.");

        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == invitation.InvitedUserEmail);
        if (user == null)
            throw new BadRequestException("You need to register an account first with the email in the invitation.");

        var teamMember = TeamMember.Create(user.Id, invitation.BusinessId, invitation.RoleId);
        context.TeamMembers.Add(teamMember);
        
        invitation.Accept();
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

        var business = await context.Businesses.FindAsync(teamMember.BusinessId) ?? throw new NotFoundException($"Business with ID {teamMember.BusinessId} not found.");
        var currentUserId = currentUserProvider.GetCurrentUserId();
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

        var business = await context.Businesses.FindAsync(teamMember.BusinessId)
            ?? throw new NotFoundException($"Business with ID {teamMember.BusinessId} not found.");

        var currentUserId = currentUserProvider.GetCurrentUserId();
        if (business.OwnerId != currentUserId)
            throw new ForbidException("Only the business owner can update team member roles.");

        if (teamMember.UserId == business.OwnerId)
            throw new BadRequestException("The business owner's role cannot be changed.");

        teamMember.UpdateRole(newRoleId);
        await context.SaveChangesAsync();
    }

    private async Task SendInvitationEmailAsync(string email, string businessName, string token)
    {
        var invitationLink = $"https://sharpinvoice.com/accept-invitation?token={Uri.EscapeDataString(token)}";
        var subject = $"You've been invited to join {businessName} on SharpInvoice";
        var message = $@"
            <h2>You've been invited!</h2>
            <p>Hello,</p>
            <p>You've been invited to join {businessName} on SharpInvoice. Click the link below to accept the invitation:</p>
            <p><a href='{invitationLink}'>Accept Invitation</a></p>
            <p>If you don't have a SharpInvoice account yet, you'll need to create one with this email address.</p>
            <p>This invitation will expire in 7 days.</p>
            <p>Thanks,<br>The SharpInvoice Team</p>
        ";

        await emailSender.SendEmailAsync(email, subject, message);
    }

    private async Task SendTeamMemberAddedEmailAsync(string email, string businessName)
    {
        var loginLink = "https://sharpinvoice.com/login";
        var subject = $"You've been added to {businessName} on SharpInvoice";
        var message = $@"
            <h2>You've been added to a team!</h2>
            <p>Hello,</p>
            <p>You've been added as a team member to {businessName} on SharpInvoice. You can log in using your existing credentials to access this business.</p>
            <p><a href='{loginLink}'>Log in to SharpInvoice</a></p>
            <p>Thanks,<br>The SharpInvoice Team</p>
        ";

        await emailSender.SendEmailAsync(email, subject, message);
    }
}