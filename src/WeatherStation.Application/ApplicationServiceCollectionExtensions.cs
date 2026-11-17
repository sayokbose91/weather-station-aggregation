using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using WeatherStation.Application.Features.Measurements;
using WeatherStation.Application.Features.Measurements.Parsing;
using WeatherStation.Application.Features.Measurements.Validation;
using WeatherStation.Application.Messaging.Wolverine;

namespace WeatherStation.Application;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<MeasurementLineValidator>(ServiceLifetime.Singleton, includeInternalTypes: true);
        services.AddSingleton<MeasurementLineParser>();
        services.AddSingleton<IMeasurementAggregator, MeasurementAggregator>();
        services.AddWolverineMessaging();
        return services;
    }
}
