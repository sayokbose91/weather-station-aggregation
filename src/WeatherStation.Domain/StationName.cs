using System.Globalization;

namespace WeatherStation.Domain;

public static class StationName
{
    public const int MaxUtf8Bytes = 100;

    public static string ToKey(string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        return name.ToUpperInvariant();
    }

    public static string ToDisplayName(string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(name.ToLowerInvariant());
    }
}
