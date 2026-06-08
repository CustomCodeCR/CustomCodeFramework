using System.Data;
using Dapper;

namespace CustomCodeFramework.Postgres.Dapper.TypeHandlers;

public sealed class GuidTypeHandler : SqlMapper.TypeHandler<Guid>
{
    public override Guid Parse(object value)
    {
        return value switch
        {
            Guid guid => guid,
            _ => Guid.Parse(value.ToString()!),
        };
    }

    public override void SetValue(IDbDataParameter parameter, Guid value)
    {
        parameter.Value = value;
    }
}
