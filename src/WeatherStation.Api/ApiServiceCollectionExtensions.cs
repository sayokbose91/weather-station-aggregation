using WeatherStation.Api.Common.Authentication;
using WeatherStation.Api.Common.ExceptionHandling;

namespace WeatherStation.Api;

public static class ApiServiceCollectionExtensions
{
    public static IServiceCollection AddApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers();
        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddApiKeyAuthentication(configuration);
        return services;
    }
}
