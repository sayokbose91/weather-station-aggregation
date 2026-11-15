using WeatherStation.Domain;

namespace WeatherStation.Application.Features.Measurements;

public interface IMeasurementAggregator
{
    Task<IReadOnlyDictionary<string, StationStatistics>> AggregateAsync(Stream input, CancellationToken cancellationToken);
}
