using System.Security.Claims;
using System.Text.Encodings.Web;
using api.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace api.Security;

public class ApiKeyAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IOptions<SecurityOptions> securityOptions) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    private readonly SecurityOptions _securityOptions = securityOptions.Value;

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(ApiKeyAuthenticationDefaults.HeaderName, out var providedApiKey))
        {
            return Task.FromResult(AuthenticateResult.Fail("API key is missing."));
        }

        if (string.IsNullOrWhiteSpace(_securityOptions.ApiKey) ||
            !string.Equals(providedApiKey.ToString(), _securityOptions.ApiKey, StringComparison.Ordinal))
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid API key."));
        }

        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "paylite-client") };
        var identity = new ClaimsIdentity(claims, ApiKeyAuthenticationDefaults.SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, ApiKeyAuthenticationDefaults.SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
