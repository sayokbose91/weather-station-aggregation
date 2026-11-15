using Microsoft.Extensions.DependencyInjection;
using WeatherStation.Application.Features.Measurements;

namespace WeatherStation.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<IMeasurementAggregator, MeasurementAggregator>();
        return services;
    }
}
