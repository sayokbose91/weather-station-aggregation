namespace WeatherStation.Application.Features.Measurements.Parsing;

internal readonly record struct Measurement(string StationName, decimal Temperature);
