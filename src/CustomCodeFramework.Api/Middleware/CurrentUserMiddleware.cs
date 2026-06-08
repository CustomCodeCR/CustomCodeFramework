using Microsoft.AspNetCore.Http;

namespace CustomCodeFramework.Api.Middleware;

public sealed class CurrentUserMiddleware(RequestDelegate next)
{
    public const string UserIdItemKey = "CurrentUser.UserId";
    public const string UserNameItemKey = "CurrentUser.UserName";
    public const string EmailItemKey = "CurrentUser.Email";

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            context.Items[UserIdItemKey] =
                context.User.FindFirst("sub")?.Value
                ?? context.User.FindFirst("user_id")?.Value
                ?? context.User.FindFirst("nameidentifier")?.Value;

            context.Items[UserNameItemKey] = context.User.Identity.Name;
            context.Items[EmailItemKey] = context.User.FindFirst("email")?.Value;
        }

        await next(context);
    }
}
