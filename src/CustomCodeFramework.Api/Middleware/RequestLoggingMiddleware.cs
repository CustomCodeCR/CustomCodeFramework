using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace CustomCodeFramework.Api.Middleware;

public sealed class RequestLoggingMiddleware(
    RequestDelegate next,
    ILogger<RequestLoggingMiddleware> logger
)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var startedAt = DateTime.UtcNow;

        logger.LogInformation(
            "HTTP request started {Method} {Path}",
            context.Request.Method,
            context.Request.Path
        );

        await next(context);

        var elapsedMs = (DateTime.UtcNow - startedAt).TotalMilliseconds;

        logger.LogInformation(
            "HTTP request completed {Method} {Path} {StatusCode} in {ElapsedMilliseconds} ms",
            context.Request.Method,
            context.Request.Path,
            context.Response.StatusCode,
            elapsedMs
        );
    }
}
