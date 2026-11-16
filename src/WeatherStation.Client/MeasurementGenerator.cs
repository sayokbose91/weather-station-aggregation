using WeatherStation.Client.Common;
using WeatherStation.Client.Models;

namespace WeatherStation.Client;

internal static class MeasurementGenerator
{
    public static IEnumerable<Measurement> Generate(long count)
    {
        var random = new Random(42);

        for (long i = 0; i < count; i++)
        {
            var (name, average) = Stations.All[random.Next(Stations.All.Length)];
            var temperature = average + random.Next(-150, 151) * 0.1m;

            yield return new Measurement(RandomCase(random, name), temperature);
        }
    }

    private static string RandomCase(Random random, string name)
    {
        var casing = random.Next(3);
        if (casing == 0)
        {
            return name.ToUpperInvariant();
        }
        else if (casing == 1)
        {
            return name.ToLowerInvariant();
        }
        else
        {
            return name;
        }
    }
}
