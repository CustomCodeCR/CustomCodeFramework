using Microsoft.AspNetCore.Http;

namespace CustomCodeFramework.Api.ProblemDetails;

public static class ProblemDetailsFactory
{
    public static Microsoft.AspNetCore.Mvc.ProblemDetails Create(
        HttpContext httpContext,
        int statusCode,
        string title,
        string detail,
        string? type = null,
        string? code = null
    )
    {
        var problemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Type = type,
            Instance = httpContext.Request.Path,
        };

        problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;

        if (!string.IsNullOrWhiteSpace(code))
        {
            problemDetails.Extensions["code"] = code;
        }

        if (httpContext.Request.Headers.TryGetValue("X-Correlation-Id", out var correlationId))
        {
            problemDetails.Extensions["correlationId"] = correlationId.ToString();
        }

        return problemDetails;
    }
}
