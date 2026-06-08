using Dapper;

namespace CustomCodeFramework.Postgres.Dapper.Mapping;

public static class DapperMappingConfiguration
{
    public static void RegisterSnakeCaseMapping<T>()
    {
        SqlMapper.SetTypeMap(typeof(T), new PascalCaseTypeMap(typeof(T)));
    }

    public static void RegisterSnakeCaseMapping(params Type[] types)
    {
        ArgumentNullException.ThrowIfNull(types);

        foreach (var type in types)
        {
            SqlMapper.SetTypeMap(type, new PascalCaseTypeMap(type));
        }
    }
}
