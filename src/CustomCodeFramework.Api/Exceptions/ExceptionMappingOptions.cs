namespace CustomCodeFramework.Api.Exceptions;

public sealed class ExceptionMappingOptions
{
    public Dictionary<Type, ExceptionMapping> Mappings { get; } = [];

    public ExceptionMappingOptions Map<TException>(int statusCode, string title, string code)
        where TException : Exception
    {
        Mappings[typeof(TException)] = new ExceptionMapping
        {
            StatusCode = statusCode,
            Title = title,
            Code = code,
        };

        return this;
    }
}

public sealed record ExceptionMapping
{
    public required int StatusCode { get; init; }

    public required string Title { get; init; }

    public required string Code { get; init; }
}
