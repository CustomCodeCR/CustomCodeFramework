namespace CustomCodeFramework.Messaging.Events;

public static class IntegrationEventNameResolver
{
    public static string Resolve<TEvent>()
    {
        return Resolve(typeof(TEvent));
    }

    public static string Resolve(Type eventType)
    {
        ArgumentNullException.ThrowIfNull(eventType);

        var attribute = eventType
            .GetCustomAttributes(typeof(IntegrationEventNameAttribute), false)
            .Cast<IntegrationEventNameAttribute>()
            .FirstOrDefault();

        if (attribute is not null && !string.IsNullOrWhiteSpace(attribute.Name))
        {
            return attribute.Name;
        }

        return ToKebabCase(eventType.Name);
    }

    private static string ToKebabCase(string value)
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
                    chars.Add('-');
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
