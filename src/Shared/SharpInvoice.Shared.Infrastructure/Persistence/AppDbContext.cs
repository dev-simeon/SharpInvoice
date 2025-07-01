namespace SharpInvoice.Shared.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using SharpInvoice.Modules.Auth.Domain.Entities;
using SharpInvoice.Modules.Invoicing.Domain.Entities;
using SharpInvoice.Modules.Payments.Domain.Entities;
using SharpInvoice.Modules.UserManagement.Domain.Entities;
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
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
}
