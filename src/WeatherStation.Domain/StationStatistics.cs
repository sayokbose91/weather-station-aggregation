namespace WeatherStation.Domain;

public sealed class StationStatistics
{
    public decimal Min { get; set; }

    public decimal Max { get; set; }

    public decimal Sum { get; set; }

    public long Count { get; set; }

    public decimal Mean => Math.Round(Sum / Count, 1, MidpointRounding.AwayFromZero);
}
