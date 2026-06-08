using Microsoft.AspNetCore.Http;

namespace CustomCodeFramework.Api.ProblemDetails;

public static class ProblemDetailsExtensions
{
    public static IResult ToResult(this Microsoft.AspNetCore.Mvc.ProblemDetails problemDetails)
    {
        return Results.Problem(
            title: problemDetails.Title,
            detail: problemDetails.Detail,
            type: problemDetails.Type,
            statusCode: problemDetails.Status,
            instance: problemDetails.Instance,
            extensions: problemDetails.Extensions
        );
    }
}
