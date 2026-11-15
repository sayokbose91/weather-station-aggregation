using System.Text;
using System.Text.Json;
using WeatherStation.Domain;

namespace WeatherStation.Application.Features.Measurements.Parsing;

internal static class MeasurementLineParser
{
    private const string TemperatureInvalid = "'temperature' must be a number between -99.9 and 99.9 with one fractional digit.";

    private static readonly string NameLengthInvalid = $"'stationName' must be 1-{StationName.MaxUtf8Bytes} UTF-8 bytes.";

    private static readonly JsonSerializerOptions Options = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public static bool TryParse(ReadOnlySpan<byte> line, out Measurement measurement, out string error)
    {
        measurement = default;

        MeasurementLine? parsed;
        try
        {
            parsed = JsonSerializer.Deserialize<MeasurementLine>(line, Options);
        }
        catch (JsonException e)
        {
            error = "Malformed JSON.";
            if (e.Path == "$.temperature") error = TemperatureInvalid;
            if (e.Path == "$.stationName") error = "'stationName' must be a string.";
            return false;
        }

        if (parsed?.StationName is null)
        {
            error = "Missing 'stationName'.";
            return false;
        }

        if (parsed.Temperature is null)
        {
            error = "Missing 'temperature'.";
            return false;
        }

        var nameBytes = Encoding.UTF8.GetByteCount(parsed.StationName);
        if (nameBytes is 0 or > StationName.MaxUtf8Bytes)
        {
            error = NameLengthInvalid;
            return false;
        }

        if (!Temperature.IsValid(parsed.Temperature.Value))
        {
            error = TemperatureInvalid;
            return false;
        }

        measurement = new Measurement(parsed.StationName, parsed.Temperature.Value);
        error = "";
        return true;
    }

    private sealed record MeasurementLine(string? StationName, decimal? Temperature);
}
