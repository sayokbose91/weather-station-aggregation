using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WeatherStation.Application.Interfaces.Repositories;
using WeatherStation.Contracts;
using WeatherStation.Domain;
using WeatherStation.Domain.Models;

namespace WeatherStation.Application.Features.Measurements.Commands;

public static class AggregateMeasurements
{
    public sealed record Command(Stream Body);

    public sealed record Response(IList<StationDto> Stations);

    public sealed class Handler(IMeasurementAggregator aggregator, IServiceScopeFactory scopeFactory, ILogger<Handler> logger)
    {
        public async Task<Response> Handle(Command command, CancellationToken ct)
        {
            var result = await aggregator.AggregateAsync(command.Body, ct);

            if (result.FailedMeasurements.Count > 0)
            {
                _ = Task.Run(() => SaveFailedMeasurementsAsync(result.FailedMeasurements), CancellationToken.None);
            }

            var stations = result.Stations
                .Select(t => new StationDto(StationName.ToDisplayName(t.Key), t.Value.Min, t.Value.Mean, t.Value.Max))
                .OrderBy(s => s.StationName, StringComparer.Ordinal)
                .ToList();

            return new Response(stations);
        }

        private async Task SaveFailedMeasurementsAsync(IList<FailedMeasurement> failedMeasurements)
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var repository = scope.ServiceProvider.GetRequiredService<IMeasurementRepository>();
                await repository.AddFailedMeasurementsAsync(failedMeasurements, CancellationToken.None);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Could not save {Count} failed measurements", failedMeasurements.Count);
            }
        }
    }
}
