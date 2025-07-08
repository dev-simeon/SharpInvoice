namespace SharpInvoice.Modules.UserManagement.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;
using SharpInvoice.Modules.UserManagement.Application.Interfaces;
using SharpInvoice.Modules.UserManagement.Domain.Entities;
using SharpInvoice.Shared.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class TeamMemberRepository(AppDbContext context) : ITeamMemberRepository
{
    public async Task<TeamMember?> GetByIdAsync(Guid teamMemberId)
    {
        return await context.TeamMembers.FindAsync(teamMemberId);
    }

    public async Task<TeamMember?> GetByUserIdAndBusinessIdAsync(Guid userId, Guid businessId)
    {
        return await context.TeamMembers
            .FirstOrDefaultAsync(tm => tm.UserId == userId && tm.BusinessId == businessId);
    }

    public async Task<IEnumerable<TeamMember>> GetByBusinessIdAsync(Guid businessId)
    {
        return await context.TeamMembers
            .Where(tm => tm.BusinessId == businessId)
            .Include(tm => tm.User)
            .ToListAsync();
    }

    public async Task AddAsync(TeamMember teamMember)
    {
        await context.TeamMembers.AddAsync(teamMember);
    }

    public void Remove(TeamMember teamMember)
    {
        context.TeamMembers.Remove(teamMember);
    }

    public async Task<bool> IsTeamMemberAsync(Guid userId, Guid businessId)
    {
        return await context.TeamMembers.AnyAsync(tm => tm.UserId == userId && tm.BusinessId == businessId);
    }
}