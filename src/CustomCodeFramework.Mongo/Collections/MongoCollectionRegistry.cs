namespace CustomCodeFramework.Mongo.Collections;

public static class MongoCollectionRegistry
{
    public static string GetCollectionName<TDocument>()
    {
        return GetCollectionName(typeof(TDocument));
    }

    public static string GetCollectionName(Type documentType)
    {
        ArgumentNullException.ThrowIfNull(documentType);

        var attribute = documentType
            .GetCustomAttributes(typeof(MongoCollectionNameAttribute), false)
            .Cast<MongoCollectionNameAttribute>()
            .FirstOrDefault();

        if (attribute is not null && !string.IsNullOrWhiteSpace(attribute.Name))
        {
            return attribute.Name;
        }

        return ToSnakeCase(documentType.Name);
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
