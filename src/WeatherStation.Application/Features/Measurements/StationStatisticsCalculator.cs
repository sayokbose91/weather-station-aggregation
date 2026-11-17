using WeatherStation.Domain.Models;

namespace WeatherStation.Application.Features.Measurements;

internal static class StationStatisticsCalculator
{
    public static void Add(StationStatistics stats, decimal temperature)
    {
        if (stats.Count == 0)
        {
            stats.Min = temperature;
            stats.Max = temperature;
        }
        else
        {
            stats.Min = Math.Min(stats.Min, temperature);
            stats.Max = Math.Max(stats.Max, temperature);
        }

        stats.Sum += temperature;
        stats.Count++;
    }
}
