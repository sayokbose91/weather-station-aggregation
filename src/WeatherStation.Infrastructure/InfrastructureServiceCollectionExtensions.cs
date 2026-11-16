using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WeatherStation.Application.Interfaces.Repositories;
using WeatherStation.Infrastructure.Contexts;
using WeatherStation.Infrastructure.Repositories;

namespace WeatherStation.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<WeatherStationDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("WeatherStation")));

        services.AddScoped<IMeasurementRepository, MeasurementRepository>();
        services.AddHostedService<DatabaseInitializer>();

        return services;
    }
}
