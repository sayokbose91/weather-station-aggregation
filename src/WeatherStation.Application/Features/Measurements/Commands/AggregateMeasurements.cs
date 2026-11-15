using WeatherStation.Contracts;
using WeatherStation.Domain;

namespace WeatherStation.Application.Features.Measurements.Commands;

public static class AggregateMeasurements
{
    public sealed record Command(Stream Body);

    public sealed record Response(IReadOnlyList<StationDto> Stations);

    public sealed class Handler(IMeasurementAggregator aggregator)
    {
        public async Task<Response> Handle(Command command, CancellationToken ct)
        {
            var totals = await aggregator.AggregateAsync(command.Body, ct);

            var stations = totals
                .Select(t => new StationDto(StationName.ToDisplayName(t.Key), t.Value.Min, t.Value.Mean, t.Value.Max))
                .OrderBy(s => s.StationName, StringComparer.Ordinal)
                .ToList();

            return new Response(stations);
        }
    }
}
