namespace WeatherStation.Contracts;

public sealed record StationDto(string StationName, decimal Min, decimal Mean, decimal Max);
