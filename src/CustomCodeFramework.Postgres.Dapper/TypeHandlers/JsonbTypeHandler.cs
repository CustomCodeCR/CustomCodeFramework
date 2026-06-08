using System.Data;
using System.Text.Json;
using Dapper;
using NpgsqlTypes;

namespace CustomCodeFramework.Postgres.Dapper.TypeHandlers;

public sealed class JsonbTypeHandler<TValue> : SqlMapper.TypeHandler<TValue>
{
    public override TValue Parse(object value)
    {
        if (value is TValue typedValue)
        {
            return typedValue;
        }

        return JsonSerializer.Deserialize<TValue>(value.ToString()!)
            ?? throw new InvalidOperationException(
                $"Unable to deserialize JSONB value to {typeof(TValue).Name}."
            );
    }

    public override void SetValue(IDbDataParameter parameter, TValue value)
    {
        parameter.Value = JsonSerializer.Serialize(value);

        if (parameter is Npgsql.NpgsqlParameter npgsqlParameter)
        {
            npgsqlParameter.NpgsqlDbType = NpgsqlDbType.Jsonb;
        }
    }
}
