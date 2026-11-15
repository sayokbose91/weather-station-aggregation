using Microsoft.Extensions.DependencyInjection;
using WeatherStation.Application.Features.Measurements;
using WeatherStation.Application.Messaging.Wolverine;

namespace WeatherStation.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<IMeasurementAggregator, MeasurementAggregator>();
        services.AddWolverineMessaging();
        return services;
    }
}
