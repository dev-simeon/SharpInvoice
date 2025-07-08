namespace SharpInvoice.Modules.UserManagement.Application.Interfaces;

using SharpInvoice.Modules.UserManagement.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Comprehensive team member service interface that handles all team management operations
/// </summary>
public interface ITeamMemberService
{
    // Team Member Management
    Task<IEnumerable<TeamMemberDto>> GetTeamMembersAsync(Guid businessId);
    Task InviteTeamMemberAsync(Guid businessId, string email, Guid roleId);
    Task AcceptInvitationAsync(string token);
    Task RemoveTeamMemberAsync(Guid teamMemberId);
    Task UpdateTeamMemberRoleAsync(Guid teamMemberId, Guid newRoleId);
    
    // User Roles and Permissions
    Task<IEnumerable<string>> GetUserRolesAsync(Guid userId, Guid businessId);
    Task<IEnumerable<string>> GetUserPermissionsAsync(Guid userId, Guid businessId);
}