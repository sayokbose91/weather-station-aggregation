using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using WeatherStation.Contracts;

namespace WeatherStation.Api.Common.Authentication;

internal sealed class ApiKeyAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory loggerFactory,
    UrlEncoder encoder,
    IOptions<ApiKeyOptions> apiKey)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, loggerFactory, encoder)
{
    public const string SchemeName = "ApiKey";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var provided = Encoding.UTF8.GetBytes(Request.Headers[ApiRoutes.ApiKeyHeader].ToString());
        var expected = Encoding.UTF8.GetBytes(apiKey.Value.Value);

        if (!CryptographicOperations.FixedTimeEquals(provided, expected))
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid API key."));
        }

        var identity = new ClaimsIdentity(SchemeName);
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName)));
    }
}
