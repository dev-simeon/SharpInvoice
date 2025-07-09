namespace SharpInvoice.Core.Domain.Shared;

public abstract class AuditableEntity<TId> : Entity<TId> where TId : notnull
{
    public DateTime CreatedAt { get; internal set; }
    public string? CreatedBy { get; internal set; } // Can be UserId or username
    public DateTime? UpdatedAt { get; internal set; }
    public string? UpdatedBy { get; internal set; }

    protected AuditableEntity(TId id) : base(id) { }
    protected AuditableEntity() { } // EF Core
}