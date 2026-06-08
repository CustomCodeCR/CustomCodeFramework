using System.Data;
using System.Reflection;
using Dapper;

namespace CustomCodeFramework.Postgres.Dapper.TypeHandlers;

public sealed class StronglyTypedIdTypeHandler<TStronglyTypedId, TValue>
    : SqlMapper.TypeHandler<TStronglyTypedId>
    where TStronglyTypedId : notnull
    where TValue : notnull
{
    public override TStronglyTypedId Parse(object value)
    {
        var typedValue = ConvertValue(value);
        var constructor = typeof(TStronglyTypedId).GetConstructor([typeof(TValue)]);

        if (constructor is null)
        {
            throw new InvalidOperationException(
                $"Type {typeof(TStronglyTypedId).Name} must have a constructor with parameter {typeof(TValue).Name}."
            );
        }

        return (TStronglyTypedId)constructor.Invoke([typedValue]);
    }

    public override void SetValue(IDbDataParameter parameter, TStronglyTypedId value)
    {
        var property = typeof(TStronglyTypedId).GetProperty(
            "Value",
            BindingFlags.Public | BindingFlags.Instance
        );

        if (property is null)
        {
            throw new InvalidOperationException(
                $"Type {typeof(TStronglyTypedId).Name} must expose a public Value property."
            );
        }

        parameter.Value = property.GetValue(value);
    }

    private static TValue ConvertValue(object value)
    {
        if (value is TValue typedValue)
        {
            return typedValue;
        }

        return (TValue)Convert.ChangeType(value, typeof(TValue));
    }
}
