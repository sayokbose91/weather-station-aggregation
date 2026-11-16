namespace WeatherStation.Domain.Models;

public class FailedMeasurement
{
    public long Id { get; set; }

    public string Line { get; set; } = null!;

    public string Reason { get; set; } = null!;

    public DateTime FailedAt { get; set; }
}
