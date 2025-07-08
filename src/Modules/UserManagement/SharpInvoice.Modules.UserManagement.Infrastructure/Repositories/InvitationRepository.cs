namespace SharpInvoice.Modules.UserManagement.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;
using SharpInvoice.Modules.UserManagement.Application.Interfaces;
using SharpInvoice.Modules.UserManagement.Domain.Entities;
using SharpInvoice.Shared.Infrastructure.Persistence;
using System;
using System.Threading.Tasks;

public class InvitationRepository(AppDbContext context) : IInvitationRepository
{
    public async Task<Invitation?> GetByIdAsync(Guid invitationId)
    {
        return await context.Invitations
            .Include(i => i.Business)
            .FirstOrDefaultAsync(i => i.Id == invitationId);
    }

    public async Task<Invitation?> GetByTokenAsync(string token)
    {
        return await context.Invitations
            .Include(i => i.Business)
            .FirstOrDefaultAsync(i => i.Token == token && i.Status == InvitationStatus.Pending && i.ExpiryDate > DateTime.UtcNow);
    }

    public async Task AddAsync(Invitation invitation)
    {
        await context.Invitations.AddAsync(invitation);
    }
}