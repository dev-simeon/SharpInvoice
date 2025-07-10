using Microsoft.EntityFrameworkCore;
using SharpInvoice.Core.Domain.Entities;
using SharpInvoice.Core.Domain.Enums;
using SharpInvoice.Core.Interfaces.Repositories;
using SharpInvoice.Infrastructure.Persistence;

namespace SharpInvoice.Infrastructure.Repositories;

public class InvitationRepository : IInvitationRepository
{
    private readonly AppDbContext _db;

    public InvitationRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Invitation?> GetByIdAsync(string id)
    {
        return await _db.Invitations
            .Include(i => i.Business)
            .Include(i => i.Role)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<Invitation?> GetByTokenAsync(string token)
    {
        return await _db.Invitations
            .Include(i => i.Business)
            .Include(i => i.Role)
            .FirstOrDefaultAsync(i => i.Token == token);
    }

    public async Task<IEnumerable<Invitation>> GetByBusinessIdAsync(string businessId)
    {
        return await _db.Invitations
            .Include(i => i.Role)
            .Where(i => i.BusinessId == businessId)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Invitation>> GetByEmailAsync(string email)
    {
        return await _db.Invitations
            .Include(i => i.Business)
            .Include(i => i.Role)
            .Where(i => i.InvitedUserEmail.ToLower() == email.ToLower())
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Invitation>> GetByStatusAsync(string businessId, InvitationStatus status)
    {
        return await _db.Invitations
            .Include(i => i.Role)
            .Where(i => i.BusinessId == businessId && i.Status == status)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(string id)
    {
        return await _db.Invitations.AnyAsync(i => i.Id == id);
    }

    public async Task AddAsync(Invitation invitation)
    {
        await _db.Invitations.AddAsync(invitation);
    }

    public Task UpdateAsync(Invitation invitation)
    {
        _db.Invitations.Update(invitation);
        return Task.CompletedTask;
    }

    public async Task ExpireAllPendingInvitationsAsync()
    {
        var expiredInvitations = await _db.Invitations
            .Where(i => i.Status == InvitationStatus.Pending && i.ExpiryDate < DateTime.UtcNow)
            .ToListAsync();

        foreach (var invitation in expiredInvitations)
        {
            invitation.Expire();
        }
    }

    public async Task SaveChangesAsync()
    {
        await _db.SaveChangesAsync();
    }
} 