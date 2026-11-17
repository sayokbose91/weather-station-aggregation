using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WeatherStation.Infrastructure.Contexts;

namespace WeatherStation.Infrastructure;

public class DatabaseInitializer(IServiceScopeFactory scopeFactory) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<WeatherStationDbContext>();
        await context.Database.EnsureCreatedAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
