using WeatherStation.Domain;

namespace WeatherStation.Application.Features.Measurements;

public interface IMeasurementAggregator
{
    Task<IDictionary<string, StationStatistics>> AggregateAsync(Stream input, CancellationToken cancellationToken);
}
