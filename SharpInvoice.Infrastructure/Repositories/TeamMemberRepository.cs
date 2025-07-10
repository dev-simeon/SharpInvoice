using Microsoft.EntityFrameworkCore;
using SharpInvoice.Core.Domain.Entities;
using SharpInvoice.Core.Interfaces.Repositories;
using SharpInvoice.Infrastructure.Persistence;

namespace SharpInvoice.Infrastructure.Repositories;

public class TeamMemberRepository : ITeamMemberRepository
{
    private readonly AppDbContext _db;

    public TeamMemberRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<TeamMember?> GetAsync(string userId, string businessId)
    {
        return await _db.TeamMembers
            .Include(tm => tm.User)
            .Include(tm => tm.Role)
            .FirstOrDefaultAsync(tm => tm.UserId == userId && tm.BusinessId == businessId);
    }

    public async Task<IEnumerable<TeamMember>> GetByBusinessIdAsync(string businessId)
    {
        return await _db.TeamMembers
            .Include(tm => tm.User)
            .Include(tm => tm.Role)
            .Where(tm => tm.BusinessId == businessId)
            .OrderBy(tm => tm.User.LastName)
            .ThenBy(tm => tm.User.FirstName)
            .ToListAsync();
    }

    public async Task<IEnumerable<TeamMember>> GetByUserIdAsync(string userId)
    {
        return await _db.TeamMembers
            .Include(tm => tm.Business)
            .Include(tm => tm.Role)
            .Where(tm => tm.UserId == userId)
            .OrderBy(tm => tm.Business.Name)
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(string userId, string businessId)
    {
        return await _db.TeamMembers
            .AnyAsync(tm => tm.UserId == userId && tm.BusinessId == businessId);
    }

    public async Task AddAsync(TeamMember teamMember)
    {
        await _db.TeamMembers.AddAsync(teamMember);
    }

    public Task UpdateAsync(TeamMember teamMember)
    {
        _db.TeamMembers.Update(teamMember);
        return Task.CompletedTask;
    }

    public async Task RemoveAsync(string userId, string businessId)
    {
        var teamMember = await _db.TeamMembers
            .FirstOrDefaultAsync(tm => tm.UserId == userId && tm.BusinessId == businessId);
            
        if (teamMember != null)
        {
            _db.TeamMembers.Remove(teamMember);
        }
    }

    public async Task SaveChangesAsync()
    {
        await _db.SaveChangesAsync();
    }
} 