using System.Data;
using Dapper;

namespace CustomCodeFramework.Postgres.Dapper.TypeHandlers;

public sealed class TimeOnlyTypeHandler : SqlMapper.TypeHandler<TimeOnly>
{
    public override TimeOnly Parse(object value)
    {
        return value switch
        {
            TimeOnly timeOnly => timeOnly,
            TimeSpan timeSpan => TimeOnly.FromTimeSpan(timeSpan),
            _ => TimeOnly.Parse(value.ToString()!),
        };
    }

    public override void SetValue(IDbDataParameter parameter, TimeOnly value)
    {
        parameter.Value = value;
    }
}
