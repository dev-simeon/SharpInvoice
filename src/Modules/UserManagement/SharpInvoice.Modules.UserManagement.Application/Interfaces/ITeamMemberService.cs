namespace SharpInvoice.Modules.UserManagement.Application.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using SharpInvoice.Modules.UserManagement.Application.Dtos;

public interface ITeamMemberService
{
    Task<IEnumerable<string>> GetUserRolesAsync(Guid userId, Guid businessId);
    Task InviteTeamMemberAsync(Guid businessId, string email, Guid roleId);
    Task AcceptInvitationAsync(string token);
    Task<IEnumerable<TeamMemberDto>> GetTeamMembersForBusinessAsync(Guid businessId);
    Task RemoveTeamMemberAsync(Guid teamMemberId);
    Task UpdateTeamMemberRoleAsync(Guid teamMemberId, Guid newRoleId);
} 