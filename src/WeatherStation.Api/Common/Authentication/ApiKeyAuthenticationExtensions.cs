using Microsoft.AspNetCore.Authentication;

namespace WeatherStation.Api.Common.Authentication;

public static class ApiKeyAuthenticationExtensions
{
    public static IServiceCollection AddApiKeyAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<ApiKeyOptions>()
            .Bind(configuration.GetSection(ApiKeyOptions.SectionName))
            .Validate(o => !string.IsNullOrWhiteSpace(o.Value), "Set ApiKey:Value with user-secrets or the ApiKey__Value environment variable.")
            .ValidateOnStart();

        services.AddAuthentication(ApiKeyAuthenticationHandler.SchemeName)
            .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(ApiKeyAuthenticationHandler.SchemeName, null);

        services.AddAuthorization();

        return services;
    }
}
