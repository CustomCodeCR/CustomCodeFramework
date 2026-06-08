using CustomCodeFramework.Api.Exceptions;
using CustomCodeFramework.Api.Middleware;
using CustomCodeFramework.Api.Swagger;
using CustomCodeFramework.Api.Versioning;
using CustomCodeFramework.Core.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace CustomCodeFramework.Api.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCustomCodeApi(
        this IServiceCollection services,
        Action<ExceptionMappingOptions>? configureExceptionMappings = null
    )
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddHttpContextAccessor();

        services.AddProblemDetails();

        services.AddExceptionHandler<GlobalExceptionHandler>();

        services.Configure<ExceptionMappingOptions>(options =>
        {
            configureExceptionMappings?.Invoke(options);
        });

        services.AddScoped<ICorrelationContext, HttpCorrelationContext>();

        services.AddCustomCodeApiVersioning();

        return services;
    }

    public static IServiceCollection AddCustomCodeApiWithSwagger(
        this IServiceCollection services,
        string title = "API",
        string version = "v1",
        Action<ExceptionMappingOptions>? configureExceptionMappings = null
    )
    {
        services.AddCustomCodeApi(configureExceptionMappings);
        services.AddCustomCodeSwagger(title, version);

        return services;
    }

    public static IApplicationBuilder UseCustomCodeApi(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.UseMiddleware<CorrelationIdMiddleware>();
        app.UseMiddleware<RequestLoggingMiddleware>();
        app.UseMiddleware<CurrentUserMiddleware>();

        app.UseExceptionHandler();

        return app;
    }
}

internal sealed class HttpCorrelationContext(IHttpContextAccessor httpContextAccessor)
    : ICorrelationContext
{
    public string? CorrelationId
    {
        get
        {
            var httpContext = httpContextAccessor.HttpContext;

            if (httpContext is null)
            {
                return null;
            }

            if (httpContext.Items.TryGetValue(CorrelationIdMiddleware.HeaderName, out var value))
            {
                return value?.ToString();
            }

            if (
                httpContext.Request.Headers.TryGetValue(
                    CorrelationIdMiddleware.HeaderName,
                    out var headerValue
                )
            )
            {
                return headerValue.ToString();
            }

            return null;
        }
    }
}
