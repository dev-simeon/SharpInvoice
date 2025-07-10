using Microsoft.EntityFrameworkCore;
using SharpInvoice.Core.Domain.Entities;
using SharpInvoice.Core.Interfaces.Repositories;
using SharpInvoice.Infrastructure.Persistence;

namespace SharpInvoice.Infrastructure.Repositories;

/// <summary>
/// Repository for managing TeamMember entities.
/// </summary>
public class TeamMemberRepository(AppDbContext db) : BaseRepository<TeamMember>(db), ITeamMemberRepository
{
    /// <summary>
    /// Gets a team member by user ID and business ID.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="businessId">The business ID.</param>
    /// <returns>The team member entity if found, null otherwise.</returns>
    public async Task<TeamMember?> GetAsync(string userId, string businessId)
    {
        return await DbSet
            .Include(tm => tm.User)
            .Include(tm => tm.Role)
            .FirstOrDefaultAsync(tm => tm.UserId == userId && tm.BusinessId == businessId);
    }

    /// <summary>
    /// Gets all team members for a specific business.
    /// </summary>
    /// <param name="businessId">The business ID.</param>
    /// <returns>A collection of team members for the specified business.</returns>
    public async Task<IEnumerable<TeamMember>> GetByBusinessIdAsync(string businessId)
    {
        return await DbSet
            .Include(tm => tm.User)
            .Include(tm => tm.Role)
            .Where(tm => tm.BusinessId == businessId)
            .OrderBy(tm => tm.User.LastName)
            .ThenBy(tm => tm.User.FirstName)
            .ToListAsync();
    }

    /// <summary>
    /// Gets all team memberships for a specific user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <returns>A collection of team memberships for the specified user.</returns>
    public async Task<IEnumerable<TeamMember>> GetByUserIdAsync(string userId)
    {
        return await DbSet
            .Include(tm => tm.Business)
            .Include(tm => tm.Role)
            .Where(tm => tm.UserId == userId)
            .OrderBy(tm => tm.Business.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Checks if a team membership exists for the specified user and business.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="businessId">The business ID.</param>
    /// <returns>True if the team membership exists, false otherwise.</returns>
    public async Task<bool> ExistsAsync(string userId, string businessId)
    {
        return await DbSet
            .AnyAsync(tm => tm.UserId == userId && tm.BusinessId == businessId);
    }

    /// <summary>
    /// Removes a team membership for the specified user and business.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="businessId">The business ID.</param>
    public async Task RemoveAsync(string userId, string businessId)
    {
        var teamMember = await DbSet
            .FirstOrDefaultAsync(tm => tm.UserId == userId && tm.BusinessId == businessId);
            
        if (teamMember != null)
        {
            DbSet.Remove(teamMember);
        }
    }
} 