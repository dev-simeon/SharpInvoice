namespace SharpInvoice.Shared.Infrastructure.Persistence;

using SharpInvoice.Shared.Infrastructure.Interfaces;
using System.Threading.Tasks;
using System.Threading;

/// <summary>
/// Implementation of the Unit of Work pattern.
/// </summary>
public sealed class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="UnitOfWork"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}