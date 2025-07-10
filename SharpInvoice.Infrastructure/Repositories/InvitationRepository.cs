using Microsoft.EntityFrameworkCore;
using SharpInvoice.Core.Domain.Entities;
using SharpInvoice.Core.Domain.Enums;
using SharpInvoice.Core.Interfaces.Repositories;
using SharpInvoice.Infrastructure.Persistence;

namespace SharpInvoice.Infrastructure.Repositories;

/// <summary>
/// Repository for managing Invitation entities.
/// </summary>
public class InvitationRepository(AppDbContext db) : BaseRepository<Invitation>(db), IInvitationRepository
{
    /// <summary>
    /// Gets an invitation by its unique identifier.
    /// </summary>
    /// <param name="id">The invitation ID.</param>
    /// <returns>The invitation entity if found, null otherwise.</returns>
    public async Task<Invitation?> GetByIdAsync(string id)
    {
        return await DbSet
            .Include(i => i.Business)
            .Include(i => i.Role)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    /// <summary>
    /// Gets an invitation by its token.
    /// </summary>
    /// <param name="token">The invitation token.</param>
    /// <returns>The invitation entity if found, null otherwise.</returns>
    public async Task<Invitation?> GetByTokenAsync(string token)
    {
        return await DbSet
            .Include(i => i.Business)
            .Include(i => i.Role)
            .FirstOrDefaultAsync(i => i.Token == token);
    }

    /// <summary>
    /// Gets all invitations for a specific business.
    /// </summary>
    /// <param name="businessId">The business ID.</param>
    /// <returns>A collection of invitations for the specified business.</returns>
    public async Task<IEnumerable<Invitation>> GetByBusinessIdAsync(string businessId)
    {
        return await DbSet
            .Include(i => i.Role)
            .Where(i => i.BusinessId == businessId)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Gets all invitations for a specific email.
    /// </summary>
    /// <param name="email">The email address.</param>
    /// <returns>A collection of invitations for the specified email.</returns>
    public async Task<IEnumerable<Invitation>> GetByEmailAsync(string email)
    {
        return await DbSet
            .Include(i => i.Business)
            .Include(i => i.Role)
            .Where(i => i.InvitedUserEmail.Equals(email, StringComparison.CurrentCultureIgnoreCase))
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Gets all invitations with a specific status for a business.
    /// </summary>
    /// <param name="businessId">The business ID.</param>
    /// <param name="status">The invitation status.</param>
    /// <returns>A collection of invitations with the specified status.</returns>
    public async Task<IEnumerable<Invitation>> GetByStatusAsync(string businessId, InvitationStatus status)
    {
        return await DbSet
            .Include(i => i.Role)
            .Where(i => i.BusinessId == businessId && i.Status == status)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Checks if an invitation with the specified ID exists.
    /// </summary>
    /// <param name="id">The invitation ID.</param>
    /// <returns>True if the invitation exists, false otherwise.</returns>
    public async Task<bool> ExistsAsync(string id)
    {
        return await DbSet.AnyAsync(i => i.Id == id);
    }

    /// <summary>
    /// Expires all pending invitations that have passed their expiry date.
    /// </summary>
    public async Task ExpireAllPendingInvitationsAsync()
    {
        var expiredInvitations = await DbSet
            .Where(i => i.Status == InvitationStatus.Pending && i.ExpiryDate < DateTime.UtcNow)
            .ToListAsync();

        foreach (var invitation in expiredInvitations)
        {
            invitation.Expire();
        }
    }
} 