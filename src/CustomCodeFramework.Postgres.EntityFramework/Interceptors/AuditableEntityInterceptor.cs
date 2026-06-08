using CustomCodeFramework.Core.Abstractions;
using CustomCodeFramework.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CustomCodeFramework.Postgres.EntityFramework.Interceptors;

public sealed class AuditableEntityInterceptor(
    IDateTimeProvider dateTimeProvider,
    ICurrentUser currentUser
) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result
    )
    {
        ApplyAudit(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default
    )
    {
        ApplyAudit(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void ApplyAudit(DbContext? dbContext)
    {
        if (dbContext is null)
        {
            return;
        }

        var now = dateTimeProvider.UtcNow;
        var userId = currentUser.UserId;

        foreach (var entry in dbContext.ChangeTracker.Entries())
        {
            if (!IsAuditableEntity(entry.Entity.GetType()))
            {
                continue;
            }

            if (entry.State == EntityState.Added)
            {
                entry.Property(nameof(AuditableEntity<Guid>.CreatedAtUtc)).CurrentValue = now;
                entry.Property(nameof(AuditableEntity<Guid>.CreatedBy)).CurrentValue = userId;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Property(nameof(AuditableEntity<Guid>.UpdatedAtUtc)).CurrentValue = now;
                entry.Property(nameof(AuditableEntity<Guid>.UpdatedBy)).CurrentValue = userId;
            }
        }
    }

    private static bool IsAuditableEntity(Type type)
    {
        while (type is not null)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(AuditableEntity<>))
            {
                return true;
            }

            type = type.BaseType!;
        }

        return false;
    }
}
