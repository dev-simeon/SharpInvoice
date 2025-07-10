using SharpInvoice.Core.Domain.Entities;
using SharpInvoice.Core.Domain.Enums;

namespace SharpInvoice.Core.Interfaces.Repositories;

public interface IInvitationRepository
{
    Task<Invitation?> GetByIdAsync(string id);
    Task<Invitation?> GetByTokenAsync(string token);
    Task<IEnumerable<Invitation>> GetByBusinessIdAsync(string businessId);
    Task<IEnumerable<Invitation>> GetByEmailAsync(string email);
    Task<IEnumerable<Invitation>> GetByStatusAsync(string businessId, InvitationStatus status);
    Task<bool> ExistsAsync(string id);
    Task AddAsync(Invitation invitation);
    Task UpdateAsync(Invitation invitation);
    Task ExpireAllPendingInvitationsAsync();
    Task SaveChangesAsync();
} 