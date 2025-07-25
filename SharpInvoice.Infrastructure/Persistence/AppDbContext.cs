using Microsoft.EntityFrameworkCore;
using SharpInvoice.Core.Entities;
using SharpInvoice.Core.Shared;
using SharpInvoice.Infrastructure.Persistence.Configurations;

namespace SharpInvoice.Infrastructure.Persistence;

/// <summary>
/// Main database context for the application.
/// </summary>
/// <remarks>
/// Initializes a new instance of the AppDbContext class.
/// </remarks>
/// <param name="options">The options to be used by the DbContext.</param>
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Business> Businesses { get; set; } = null!;
    public DbSet<Role> Roles { get; set; } = null!;
    public DbSet<Invitation> Invitations { get; set; } = null!;
    public DbSet<TeamMember> TeamMembers { get; set; } = null!;
    public DbSet<Invoice> Invoices { get; set; } = null!;
    public DbSet<InvoiceItem> InvoiceItems { get; set; } = null!;
    public DbSet<Client> Clients { get; set; } = null!;
    public DbSet<Transaction> Transactions { get; set; } = null!;
    public DbSet<PasswordResetToken> PasswordResets { get; set; } = null!;

    /// <summary>
    /// Configures the model for the context.
    /// </summary>
    /// <param name="modelBuilder">The builder being used to construct the model for this context.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(UserConfiguration).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    /// <summary>
    /// Saves all changes made in this context to the database.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The number of state entries written to the database.</returns>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            UpdateTimestamps();
            return base.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to save changes to database", ex);
        }
    }

    /// <summary>
    /// Updates timestamps for entities that inherit from BaseEntity.
    /// </summary>
    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is BaseEntity && (
                e.State == EntityState.Added ||
                e.State == EntityState.Modified));

        foreach (var entityEntry in entries)
        {
            var entity = (BaseEntity)entityEntry.Entity;

            if (entityEntry.State == EntityState.Added)
            {
                entity.SetCreatedNow();
            }
            else if (entityEntry.State == EntityState.Modified)
            {
                entity.SetUpdatedNow();
            }
        }
    }
}
