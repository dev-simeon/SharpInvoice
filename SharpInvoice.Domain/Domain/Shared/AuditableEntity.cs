namespace SharpInvoice.Core.Domain.Shared;

public abstract class AuditableEntity<TId>
{
    public DateTime CreatedAt { get; internal set; } = DateTime.UtcNow;
    public string? CreatedBy { get; internal set; } // Can be UserId or username
    public DateTime? UpdatedAt { get; internal set; }
    public string? UpdatedBy { get; internal set; }

    protected TId Id { get; }

    protected AuditableEntity(TId id)
    {
        Id = id;
    }

    protected AuditableEntity() { } // EF Core
}