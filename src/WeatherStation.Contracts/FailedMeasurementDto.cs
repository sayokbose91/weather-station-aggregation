namespace WeatherStation.Contracts;

public sealed record FailedMeasurementDto(long Id, string Line, string Reason, DateTime FailedAt);
