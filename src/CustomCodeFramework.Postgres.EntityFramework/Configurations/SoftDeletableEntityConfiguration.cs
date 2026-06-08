using CustomCodeFramework.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CustomCodeFramework.Postgres.EntityFramework.Configurations;

public abstract class SoftDeletableEntityConfiguration<TEntity, TId>
    : AuditableEntityConfiguration<TEntity, TId>
    where TEntity : SoftDeletableEntity<TId>
    where TId : notnull
{
    public override void Configure(EntityTypeBuilder<TEntity> builder)
    {
        base.Configure(builder);

        builder.Property(entity => entity.IsDeleted).IsRequired();
        builder.Property(entity => entity.DeletedAtUtc);
        builder.Property(entity => entity.DeletedBy).HasMaxLength(150);
    }
}
