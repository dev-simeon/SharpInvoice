using SharpInvoice.Core.Interfaces;
using SharpInvoice.Core.Interfaces.Repositories;
using SharpInvoice.Infrastructure.Persistence;

namespace SharpInvoice.Infrastructure;

/// <summary>
/// Implementation of the Unit of Work pattern to manage transactions and repository access.
/// </summary>
public class UnitOfWork(
    AppDbContext context,
    IUserRepository users,
    IBusinessRepository businesses,
    IClientRepository clients,
    IInvitationRepository invitations,
    IInvoiceRepository invoices,
    IPasswordResetTokenRepository passwordResetTokens,
    IRoleRepository roles,
    ITeamMemberRepository teamMembers,
    ITransactionRepository transactions) : IUnitOfWork
{
    public IUserRepository Users { get; } = users;
    public IBusinessRepository Businesses { get; } = businesses;
    public IClientRepository Clients { get; } = clients;
    public IInvitationRepository Invitations { get; } = invitations;
    public IInvoiceRepository Invoices { get; } = invoices;
    public IPasswordResetTokenRepository PasswordResetTokens { get; } = passwordResetTokens;
    public IRoleRepository Roles { get; } = roles;
    public ITeamMemberRepository TeamMembers { get; } = teamMembers;
    public ITransactionRepository Transactions { get; } = transactions;

    public async Task<int> SaveChangesAsync()
    {
        // Timestamps are now handled in AppDbContext
        return await context.SaveChangesAsync();
    }

    public async Task<bool> BeginTransactionAsync()
    {
        if (context.Database.CurrentTransaction != null)
        {
            return false;
        }

        await context.Database.BeginTransactionAsync();
        return true;
    }

    public async Task CommitTransactionAsync()
    {
        if (context.Database.CurrentTransaction == null)
        {
            throw new InvalidOperationException("No active transaction to commit");
        }

        await context.Database.CommitTransactionAsync();
    }

    public async Task RollbackTransactionAsync()
    {
        if (context.Database.CurrentTransaction != null)
        {
            await context.Database.RollbackTransactionAsync();
        }
    }

    public void Dispose()
    {
        context.Dispose();
    }
} 