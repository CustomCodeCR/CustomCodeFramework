namespace CustomCodeFramework.Core.Domain.Entities;

public abstract class AuditableEntity<TId> : Entity<TId>
    where TId : notnull
{
    protected AuditableEntity(TId id)
        : base(id) { }

    protected AuditableEntity() { }

    public DateTime CreatedAtUtc { get; protected set; }

    public string? CreatedBy { get; protected set; }

    public DateTime? UpdatedAtUtc { get; protected set; }

    public string? UpdatedBy { get; protected set; }

    public void SetCreatedAudit(DateTime createdAtUtc, string? createdBy)
    {
        CreatedAtUtc = createdAtUtc;
        CreatedBy = createdBy;
    }

    public void SetUpdatedAudit(DateTime updatedAtUtc, string? updatedBy)
    {
        UpdatedAtUtc = updatedAtUtc;
        UpdatedBy = updatedBy;
    }
}
