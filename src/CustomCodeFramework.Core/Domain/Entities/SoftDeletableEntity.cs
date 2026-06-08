namespace CustomCodeFramework.Core.Domain.Entities;

public abstract class SoftDeletableEntity<TId> : AuditableEntity<TId>
    where TId : notnull
{
    protected SoftDeletableEntity(TId id)
        : base(id) { }

    protected SoftDeletableEntity() { }

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
