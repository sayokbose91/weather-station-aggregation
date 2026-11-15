using JasperFx.CodeGeneration;
using JasperFx.CodeGeneration.Model;
using Microsoft.Extensions.DependencyInjection;
using WeatherStation.Application.Interfaces;
using Wolverine;

namespace WeatherStation.Application.Messaging.Wolverine;

internal static class WolverineExtensions
{
    internal static IServiceCollection AddWolverineMessaging(this IServiceCollection services)
    {
        services.AddScoped<IBusProvider, WolverineBusProvider>();

        services.AddWolverine(ExtensionDiscovery.Automatic, options =>
        {
            options.Discovery.IncludeAssembly(typeof(WolverineExtensions).Assembly);

            options.ServiceLocationPolicy = ServiceLocationPolicy.AllowedButWarn;
        });

        return services;
    }
}
