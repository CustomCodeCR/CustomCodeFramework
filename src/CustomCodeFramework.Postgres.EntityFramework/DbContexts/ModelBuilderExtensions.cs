using Microsoft.EntityFrameworkCore;

namespace CustomCodeFramework.Postgres.EntityFramework.DbContexts;

public static class ModelBuilderExtensions
{
    public static ModelBuilder ApplyCustomCodeConventions(this ModelBuilder modelBuilder)
    {
        modelBuilder.ApplySnakeCaseNames();
        modelBuilder.ApplyUtcDateTimeConvention();
        modelBuilder.ApplyDefaultDecimalPrecision();

        return modelBuilder;
    }

    public static ModelBuilder ApplySnakeCaseNames(this ModelBuilder modelBuilder)
    {
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            entity.SetTableName(ToSnakeCase(entity.GetTableName() ?? entity.ClrType.Name));

            foreach (var property in entity.GetProperties())
            {
                property.SetColumnName(ToSnakeCase(property.Name));
            }

            foreach (var key in entity.GetKeys())
            {
                key.SetName(ToSnakeCase(key.GetName() ?? string.Empty));
            }

            foreach (var foreignKey in entity.GetForeignKeys())
            {
                foreignKey.SetConstraintName(
                    ToSnakeCase(foreignKey.GetConstraintName() ?? string.Empty)
                );
            }

            foreach (var index in entity.GetIndexes())
            {
                index.SetDatabaseName(ToSnakeCase(index.GetDatabaseName() ?? string.Empty));
            }
        }

        return modelBuilder;
    }

    public static ModelBuilder ApplyUtcDateTimeConvention(this ModelBuilder modelBuilder)
    {
        foreach (
            var property in modelBuilder
                .Model.GetEntityTypes()
                .SelectMany(entity => entity.GetProperties())
                .Where(property =>
                    property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?)
                )
        )
        {
            property.SetValueConverter(
                new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<
                    DateTime,
                    DateTime
                >(
                    value => value.Kind == DateTimeKind.Utc ? value : value.ToUniversalTime(),
                    value => DateTime.SpecifyKind(value, DateTimeKind.Utc)
                )
            );
        }

        return modelBuilder;
    }

    public static ModelBuilder ApplyDefaultDecimalPrecision(
        this ModelBuilder modelBuilder,
        int precision = 18,
        int scale = 6
    )
    {
        foreach (
            var property in modelBuilder
                .Model.GetEntityTypes()
                .SelectMany(entity => entity.GetProperties())
                .Where(property =>
                    property.ClrType == typeof(decimal) || property.ClrType == typeof(decimal?)
                )
        )
        {
            property.SetPrecision(precision);
            property.SetScale(scale);
        }

        return modelBuilder;
    }

    private static string ToSnakeCase(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        var chars = new List<char>(value.Length + Math.Min(2, value.Length / 5));

        for (var index = 0; index < value.Length; index++)
        {
            var current = value[index];

            if (char.IsUpper(current))
            {
                if (index > 0)
                {
                    chars.Add('_');
                }

                chars.Add(char.ToLowerInvariant(current));
            }
            else
            {
                chars.Add(current);
            }
        }

        return new string(chars.ToArray());
    }
}
