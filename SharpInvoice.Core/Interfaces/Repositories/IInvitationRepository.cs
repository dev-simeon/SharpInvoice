using SharpInvoice.Core.Entities;
using SharpInvoice.Core.Enums;

namespace SharpInvoice.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for managing Invitation entities.
/// </summary>
public interface IInvitationRepository : IRepository<Invitation>
{
    /// <summary>
    /// Gets an invitation by its ID.
    /// </summary>
    /// <param name="id">The invitation ID.</param>
    /// <returns>The invitation if found; otherwise, null.</returns>
    Task<Invitation?> GetByIdAsync(string id);
    
    /// <summary>
    /// Gets an invitation by its token.
    /// </summary>
    /// <param name="token">The invitation token.</param>
    /// <returns>The invitation if found; otherwise, null.</returns>
    Task<Invitation?> GetByTokenAsync(string token);
    
    /// <summary>
    /// Gets all invitations for a specific business.
    /// </summary>
    /// <param name="businessId">The business ID.</param>
    /// <returns>A collection of invitations.</returns>
    Task<IEnumerable<Invitation>> GetByBusinessIdAsync(string businessId);
    
    /// <summary>
    /// Gets all invitations for a specific email.
    /// </summary>
    /// <param name="email">The invitee's email.</param>
    /// <returns>A collection of invitations.</returns>
    Task<IEnumerable<Invitation>> GetByEmailAsync(string email);
    
    /// <summary>
    /// Gets all invitations with a specific status for a business.
    /// </summary>
    /// <param name="businessId">The business ID.</param>
    /// <param name="status">The invitation status.</param>
    /// <returns>A collection of invitations.</returns>
    Task<IEnumerable<Invitation>> GetByStatusAsync(string businessId, InvitationStatus status);
    
    /// <summary>
    /// Checks if an invitation exists.
    /// </summary>
    /// <param name="id">The invitation ID.</param>
    /// <returns>True if the invitation exists; otherwise, false.</returns>
    Task<bool> ExistsAsync(string id);
    
    /// <summary>
    /// Expires all pending invitations that have exceeded their expiry date.
    /// </summary>
    Task ExpireAllPendingInvitationsAsync();
} 