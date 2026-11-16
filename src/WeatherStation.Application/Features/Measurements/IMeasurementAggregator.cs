namespace WeatherStation.Application.Features.Measurements;

public interface IMeasurementAggregator
{
    Task<AggregationResult> AggregateAsync(Stream input, CancellationToken cancellationToken);
}
