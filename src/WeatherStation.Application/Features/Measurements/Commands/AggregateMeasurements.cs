using Microsoft.Extensions.Logging;
using WeatherStation.Application.Interfaces.Repositories;
using WeatherStation.Contracts;
using WeatherStation.Domain;

namespace WeatherStation.Application.Features.Measurements.Commands;

public static class AggregateMeasurements
{
    public sealed record Command(Stream Body);

    public sealed record Response(IList<StationDto> Stations);

    public sealed class Handler(IMeasurementAggregator aggregator, IFailedMeasurementRepository repository, ILogger<Handler> logger)
    {
        public async Task<Response> Handle(Command command, CancellationToken ct)
        {
            var result = await aggregator.AggregateAsync(command.Body, ct);

            if (result.FailedMeasurements.Count > 0)
            {
                try
                {
                    await repository.AddFailedMeasurementsAsync(result.FailedMeasurements, ct);
                }
                catch (Exception e) when (e is not OperationCanceledException)
                {
                    logger.LogError(e, "Could not save {Count} failed measurements", result.FailedMeasurements.Count);
                }
            }

            var stations = result.Stations
                .Select(t => new StationDto(StationName.ToDisplayName(t.Key), t.Value.Min, t.Value.Mean, t.Value.Max))
                .OrderBy(s => s.StationName, StringComparer.Ordinal)
                .ToList();

            return new Response(stations);
        }
    }
}
