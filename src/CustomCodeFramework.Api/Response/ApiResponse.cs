namespace CustomCodeFramework.Api.Responses;

public sealed record ApiResponse<TData>
{
    public bool Success { get; init; } = true;

    public TData? Data { get; init; }

    public string? Message { get; init; }

    public static ApiResponse<TData> Ok(TData data, string? message = null)
    {
        return new ApiResponse<TData>
        {
            Success = true,
            Data = data,
            Message = message,
        };
    }
}
