using SharpInvoice.Core.Entities;

namespace SharpInvoice.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for managing TeamMember entities.
/// </summary>
public interface ITeamMemberRepository : IRepository<TeamMember>
{
    /// <summary>
    /// Gets a team member by user ID and business ID.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="businessId">The business ID.</param>
    /// <returns>The team member if found; otherwise, null.</returns>
    Task<TeamMember?> GetAsync(string userId, string businessId);
    
    /// <summary>
    /// Gets all team members for a specific business.
    /// </summary>
    /// <param name="businessId">The business ID.</param>
    /// <returns>A collection of team members.</returns>
    Task<IEnumerable<TeamMember>> GetByBusinessIdAsync(string businessId);
    
    /// <summary>
    /// Gets all team memberships for a specific user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <returns>A collection of team members.</returns>
    Task<IEnumerable<TeamMember>> GetByUserIdAsync(string userId);
    
    /// <summary>
    /// Checks if a team membership exists.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="businessId">The business ID.</param>
    /// <returns>True if the team membership exists; otherwise, false.</returns>
    Task<bool> ExistsAsync(string userId, string businessId);
    
    /// <summary>
    /// Removes a team member.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="businessId">The business ID.</param>
    Task RemoveAsync(string userId, string businessId);
} 