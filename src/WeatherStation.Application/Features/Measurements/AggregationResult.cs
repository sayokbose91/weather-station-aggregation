using WeatherStation.Domain;
using WeatherStation.Domain.Models;

namespace WeatherStation.Application.Features.Measurements;

public sealed record AggregationResult(IDictionary<string, StationStatistics> Stations, IList<FailedMeasurement> FailedMeasurements);
