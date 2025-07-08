namespace SharpInvoice.Shared.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using SharpInvoice.Modules.Auth.Domain.Entities;
using SharpInvoice.Modules.Invoicing.Domain.Entities;
using SharpInvoice.Modules.Payments.Domain.Entities;
using SharpInvoice.Modules.UserManagement.Domain.Entities;
using SharpInvoice.Shared.Infrastructure.Interfaces;
using SharpInvoice.Shared.Kernel.Domain;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class AppDbContext(
    DbContextOptions<AppDbContext> options,
    ICurrentUserProvider currentUserProvider) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<ExternalLogin> ExternalLogins { get; set; }
    public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<Client> Clients { get; set; }
    public DbSet<InvoiceItem> InvoiceItems { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<Business> Businesses { get; set; }
    public DbSet<TeamMember> TeamMembers { get; set; }
    public DbSet<Invitation> Invitations { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.FullName?.Contains("SharpInvoice.Modules") ?? false);

        foreach (var assembly in assemblies)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(assembly);
        }

        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Handle audit fields
        var currentUserId = currentUserProvider.GetCurrentUserId().ToString();
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<AuditableEntity<Guid>>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.CreatedBy = currentUserId;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    entry.Entity.UpdatedBy = currentUserId;
                    break;
            }
        }

        // Clear domain events since we're not using them with the new architecture
        var domainEventEntities = ChangeTracker.Entries<Entity<Guid>>()
            .Select(e => e.Entity)
            .Where(e => e.DomainEvents.Any())
            .ToArray();

        foreach (var entity in domainEventEntities)
        {
            entity.ClearDomainEvents();
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
