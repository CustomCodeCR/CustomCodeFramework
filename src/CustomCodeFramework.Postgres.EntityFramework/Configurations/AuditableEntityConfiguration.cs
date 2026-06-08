using CustomCodeFramework.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CustomCodeFramework.Postgres.EntityFramework.Configurations;

public abstract class AuditableEntityConfiguration<TEntity, TId>
    : EntityTypeConfigurationBase<TEntity, TId>
    where TEntity : AuditableEntity<TId>
    where TId : notnull
{
    public override void Configure(EntityTypeBuilder<TEntity> builder)
    {
        base.Configure(builder);

        builder.Property(entity => entity.CreatedAtUtc).IsRequired();
        builder.Property(entity => entity.CreatedBy).HasMaxLength(150);
        builder.Property(entity => entity.UpdatedAtUtc);
        builder.Property(entity => entity.UpdatedBy).HasMaxLength(150);
    }
}
