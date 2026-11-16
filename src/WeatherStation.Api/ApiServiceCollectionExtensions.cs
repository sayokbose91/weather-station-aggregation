using WeatherStation.Api.Common.Authentication;
using WeatherStation.Api.Common.ExceptionHandling;

namespace WeatherStation.Api;

public static class ApiServiceCollectionExtensions
{
    public static IServiceCollection AddApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers();
        services.AddSwaggerGen();
        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddHealthChecks();
        services.AddApiKeyAuthentication(configuration);
        return services;
    }
}
