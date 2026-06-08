using CustomCodeFramework.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CustomCodeFramework.Postgres.EntityFramework.Configurations;

public abstract class EntityTypeConfigurationBase<TEntity, TId> : IEntityTypeConfiguration<TEntity>
    where TEntity : Entity<TId>
    where TId : notnull
{
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.HasKey(entity => entity.Id);
    }
}
