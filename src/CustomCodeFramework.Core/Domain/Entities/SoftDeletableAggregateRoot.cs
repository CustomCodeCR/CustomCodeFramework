namespace CustomCodeFramework.Core.Domain.Entities;

public abstract class SoftDeletableAggregateRoot<TId> : AuditableAggregateRoot<TId>
    where TId : notnull
{
    protected SoftDeletableAggregateRoot(TId id)
        : base(id) { }

    protected SoftDeletableAggregateRoot() { }

    public bool IsDeleted { get; protected set; }

    public DateTime? DeletedAtUtc { get; protected set; }

    public string? DeletedBy { get; protected set; }

    public void MarkAsDeleted(DateTime deletedAtUtc, string? deletedBy)
    {
        if (IsDeleted)
        {
            return;
        }

        IsDeleted = true;
        DeletedAtUtc = deletedAtUtc;
        DeletedBy = deletedBy;
    }

    public void Restore()
    {
        IsDeleted = false;
        DeletedAtUtc = null;
        DeletedBy = null;
    }
}
