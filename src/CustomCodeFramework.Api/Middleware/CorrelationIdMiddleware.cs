using Microsoft.AspNetCore.Http;

namespace CustomCodeFramework.Api.Middleware;

public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    public const string HeaderName = "X-Correlation-Id";

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = GetOrCreateCorrelationId(context);

        context.Items[HeaderName] = correlationId;
        context.Response.Headers[HeaderName] = correlationId;

        await next(context);
    }

    private static string GetOrCreateCorrelationId(HttpContext context)
    {
        if (
            context.Request.Headers.TryGetValue(HeaderName, out var correlationId)
            && !string.IsNullOrWhiteSpace(correlationId)
        )
        {
            return correlationId.ToString();
        }

        return Guid.NewGuid().ToString("N");
    }
}
