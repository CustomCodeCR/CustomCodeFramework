namespace CustomCodeFramework.Api.Responses;

public sealed record ApiErrorResponse
{
    public bool Success { get; init; } = false;

    public required string Code { get; init; }

    public required string Message { get; init; }

    public IReadOnlyCollection<ApiErrorDetail> Errors { get; init; } = [];

    public string? TraceId { get; init; }

    public static ApiErrorResponse Create(
        string code,
        string message,
        string? traceId = null,
        IReadOnlyCollection<ApiErrorDetail>? errors = null
    )
    {
        return new ApiErrorResponse
        {
            Code = code,
            Message = message,
            TraceId = traceId,
            Errors = errors ?? [],
        };
    }
}

public sealed record ApiErrorDetail
{
    public required string Code { get; init; }

    public required string Message { get; init; }

    public string? PropertyName { get; init; }
}
