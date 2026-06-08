using System.Security.Claims;
using System.Text.Encodings.Web;
using CustomCodeFramework.Auth.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CustomCodeFramework.Auth.ApiKeys;

public sealed class ApiKeyAuthenticationHandler(
    IOptionsMonitor<ApiKeyOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder
) : AuthenticationHandler<ApiKeyOptions>(options, logger, encoder)
{
    public const string SchemeName = "ApiKey";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var headerName = Options.HeaderName;

        if (!Request.Headers.TryGetValue(headerName, out var apiKeyValue))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var apiKey = apiKeyValue.ToString();

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return Task.FromResult(AuthenticateResult.Fail("API key is empty."));
        }

        var keyPair = Options.Keys.FirstOrDefault(pair =>
            string.Equals(pair.Value, apiKey, StringComparison.Ordinal)
        );

        if (string.IsNullOrWhiteSpace(keyPair.Key))
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid API key."));
        }

        var claims = new[]
        {
            new Claim(CustomClaimTypes.UserId, keyPair.Key),
            new Claim(ClaimTypes.NameIdentifier, keyPair.Key),
            new Claim(CustomClaimTypes.UserName, keyPair.Key),
        };

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
