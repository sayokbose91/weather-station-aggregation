namespace WeatherStation.Application.Features.Measurements.Parsing;

internal sealed record MeasurementLine(string StationName, decimal? Temperature);
