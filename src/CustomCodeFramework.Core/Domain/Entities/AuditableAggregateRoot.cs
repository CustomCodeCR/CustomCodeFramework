namespace CustomCodeFramework.Core.Domain.Entities;

public abstract class AuditableAggregateRoot<TId> : AggregateRoot<TId>
    where TId : notnull
{
    protected AuditableAggregateRoot(TId id)
        : base(id) { }

    protected AuditableAggregateRoot() { }

    public DateTime CreatedAtUtc { get; protected set; }

    public string? CreatedBy { get; protected set; }

    public DateTime? UpdatedAtUtc { get; protected set; }

    public string? UpdatedBy { get; protected set; }

    public void MarkAsCreated(DateTime createdAtUtc, string? createdBy)
    {
        CreatedAtUtc = createdAtUtc;
        CreatedBy = createdBy;
    }

    public void MarkAsUpdated(DateTime updatedAtUtc, string? updatedBy)
    {
        UpdatedAtUtc = updatedAtUtc;
        UpdatedBy = updatedBy;
    }
}
