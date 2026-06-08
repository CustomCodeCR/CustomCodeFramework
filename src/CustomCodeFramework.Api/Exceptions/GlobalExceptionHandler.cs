using CustomCodeFramework.Api.ProblemDetails;
using CustomCodeFramework.Core.Domain.Exceptions;
using CustomCodeFramework.Validation.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CustomCodeFramework.Api.Exceptions;

public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IOptions<ExceptionMappingOptions> options
) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        var problemDetails = exception switch
        {
            ValidationException validationException => CreateValidationProblemDetails(
                httpContext,
                validationException
            ),

            BusinessRuleException businessRuleException => ProblemDetailsFactory.Create(
                httpContext,
                StatusCodes.Status400BadRequest,
                "Business rule violation",
                businessRuleException.Message,
                code: "business_rule_violation"
            ),

            DomainException domainException => ProblemDetailsFactory.Create(
                httpContext,
                StatusCodes.Status400BadRequest,
                "Domain error",
                domainException.Message,
                code: "domain_error"
            ),

            _ => CreateMappedOrDefaultProblemDetails(httpContext, exception),
        };

        if (problemDetails.Status >= StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Unhandled exception");
        }
        else
        {
            logger.LogWarning(exception, "Handled exception");
        }

        httpContext.Response.StatusCode =
            problemDetails.Status ?? StatusCodes.Status500InternalServerError;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    private static Microsoft.AspNetCore.Mvc.ProblemDetails CreateValidationProblemDetails(
        HttpContext httpContext,
        ValidationException exception
    )
    {
        var problemDetails = ProblemDetailsFactory.Create(
            httpContext,
            StatusCodes.Status400BadRequest,
            "Validation error",
            exception.Message,
            code: "validation_error"
        );

        problemDetails.Extensions["errors"] = exception
            .Errors.Select(error => new
            {
                error.PropertyName,
                error.Code,
                error.Message,
                error.AttemptedValue,
            })
            .ToArray();

        return problemDetails;
    }

    private Microsoft.AspNetCore.Mvc.ProblemDetails CreateMappedOrDefaultProblemDetails(
        HttpContext httpContext,
        Exception exception
    )
    {
        var exceptionType = exception.GetType();

        if (options.Value.Mappings.TryGetValue(exceptionType, out var mapping))
        {
            return ProblemDetailsFactory.Create(
                httpContext,
                mapping.StatusCode,
                mapping.Title,
                exception.Message,
                code: mapping.Code
            );
        }

        return ProblemDetailsFactory.Create(
            httpContext,
            StatusCodes.Status500InternalServerError,
            "Internal server error",
            "An unexpected error occurred.",
            code: "internal_server_error"
        );
    }
}
