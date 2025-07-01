namespace SharpInvoice.Shared.Kernel.Domain;

public abstract class AuditableEntity<TId> : Entity<TId> where TId : notnull
{
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; } // Can be UserId or username
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }

    protected AuditableEntity(TId id) : base(id) { }
    protected AuditableEntity() { } // EF Core
}