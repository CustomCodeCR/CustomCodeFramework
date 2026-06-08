using CustomCodeFramework.Core.Abstractions;
using CustomCodeFramework.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CustomCodeFramework.Postgres.EntityFramework.Interceptors;

public sealed class SoftDeleteInterceptor(
    IDateTimeProvider dateTimeProvider,
    ICurrentUser currentUser
) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result
    )
    {
        ApplySoftDelete(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default
    )
    {
        ApplySoftDelete(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void ApplySoftDelete(DbContext? dbContext)
    {
        if (dbContext is null)
        {
            return;
        }

        var now = dateTimeProvider.UtcNow;
        var userId = currentUser.UserId;

        foreach (var entry in dbContext.ChangeTracker.Entries())
        {
            if (entry.State != EntityState.Deleted)
            {
                continue;
            }

            if (!IsSoftDeletableEntity(entry.Entity.GetType()))
            {
                continue;
            }

            entry.State = EntityState.Modified;
            entry.Property(nameof(SoftDeletableEntity<Guid>.IsDeleted)).CurrentValue = true;
            entry.Property(nameof(SoftDeletableEntity<Guid>.DeletedAtUtc)).CurrentValue = now;
            entry.Property(nameof(SoftDeletableEntity<Guid>.DeletedBy)).CurrentValue = userId;
        }
    }

    private static bool IsSoftDeletableEntity(Type type)
    {
        while (type is not null)
        {
            if (
                type.IsGenericType
                && type.GetGenericTypeDefinition() == typeof(SoftDeletableEntity<>)
            )
            {
                return true;
            }

            type = type.BaseType!;
        }

        return false;
    }
}
