namespace SharpInvoice.Modules.UserManagement.Application.Interfaces;

using SharpInvoice.Modules.UserManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface ITeamMemberRepository
{
    Task<TeamMember?> GetByIdAsync(Guid teamMemberId);
    Task<TeamMember?> GetByUserIdAndBusinessIdAsync(Guid userId, Guid businessId);
    Task<IEnumerable<TeamMember>> GetByBusinessIdAsync(Guid businessId);
    Task AddAsync(TeamMember teamMember);
    void Remove(TeamMember teamMember);
    Task<bool> IsTeamMemberAsync(Guid userId, Guid businessId);
}