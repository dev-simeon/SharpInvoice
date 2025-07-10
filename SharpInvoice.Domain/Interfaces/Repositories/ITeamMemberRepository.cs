using SharpInvoice.Core.Domain.Entities;

namespace SharpInvoice.Core.Interfaces.Repositories;

public interface ITeamMemberRepository
{
    Task<TeamMember?> GetAsync(string userId, string businessId);
    Task<IEnumerable<TeamMember>> GetByBusinessIdAsync(string businessId);
    Task<IEnumerable<TeamMember>> GetByUserIdAsync(string userId);
    Task<bool> ExistsAsync(string userId, string businessId);
    Task AddAsync(TeamMember teamMember);
    Task UpdateAsync(TeamMember teamMember);
    Task RemoveAsync(string userId, string businessId);
    Task SaveChangesAsync();
} 