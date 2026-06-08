using Microsoft.AspNetCore.Authentication;

namespace CustomCodeFramework.Auth.ApiKeys;

public sealed class ApiKeyOptions : AuthenticationSchemeOptions
{
    public const string SectionName = "Auth:ApiKeys";

    public string HeaderName { get; init; } = "X-Api-Key";

    public Dictionary<string, string> Keys { get; init; } = [];
}
