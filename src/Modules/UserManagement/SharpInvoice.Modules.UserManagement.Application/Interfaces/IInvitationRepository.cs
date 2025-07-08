namespace SharpInvoice.Modules.UserManagement.Application.Interfaces;

using SharpInvoice.Modules.UserManagement.Domain.Entities;
using System;
using System.Threading.Tasks;

public interface IInvitationRepository
{
    Task<Invitation?> GetByIdAsync(Guid invitationId);
    Task<Invitation?> GetByTokenAsync(string token);
    Task AddAsync(Invitation invitation);
}