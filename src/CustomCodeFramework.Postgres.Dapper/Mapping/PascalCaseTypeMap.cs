using System.Reflection;
using Dapper;

namespace CustomCodeFramework.Postgres.Dapper.Mapping;

public sealed class PascalCaseTypeMap(Type type) : SqlMapper.ITypeMap
{
    private readonly CustomPropertyTypeMap _propertyTypeMap = new(
        type,
        (_, columnName) =>
            type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(property =>
                    string.Equals(property.Name, columnName, StringComparison.OrdinalIgnoreCase)
                )
    );

    private readonly DefaultTypeMap _defaultTypeMap = new(type);

    public ConstructorInfo? FindConstructor(string[] names, Type[] types)
    {
        return _defaultTypeMap.FindConstructor(names, types);
    }

    public ConstructorInfo? FindExplicitConstructor()
    {
        return _defaultTypeMap.FindExplicitConstructor();
    }

    public SqlMapper.IMemberMap? GetConstructorParameter(
        ConstructorInfo constructor,
        string columnName
    )
    {
        return _defaultTypeMap.GetConstructorParameter(constructor, columnName);
    }

    public SqlMapper.IMemberMap? GetMember(string columnName)
    {
        return _propertyTypeMap.GetMember(columnName) ?? _defaultTypeMap.GetMember(columnName);
    }
}
