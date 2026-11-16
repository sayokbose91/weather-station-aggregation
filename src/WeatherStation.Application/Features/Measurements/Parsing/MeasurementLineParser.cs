using System.Text;
using System.Text.Json;
using WeatherStation.Domain;

namespace WeatherStation.Application.Features.Measurements.Parsing;

internal static class MeasurementLineParser
{
    internal const string MalformedJson = "Malformed JSON.";
    internal const string StationNameMissing = "Missing 'stationName'.";
    internal const string TemperatureMissing = "Missing 'temperature'.";
    internal const string StationNameInvalid = "'stationName' must be a UTF-8 string of 1-100 bytes.";
    internal const string TemperatureNotANumber = "'temperature' must be a number.";
    internal const string TemperatureOutOfRange = "'temperature' must be between -99.9 and 99.9.";
    internal const string TemperatureFractionalDigit = "'temperature' must have one fractional digit.";

    private static readonly JsonSerializerOptions Options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public static bool TryParse(ReadOnlySpan<byte> line, out Measurement measurement, out string error)
    {
        measurement = default;
        error = string.Empty;

        MeasurementLine? parsed;
        try
        {
            parsed = JsonSerializer.Deserialize<MeasurementLine>(line, Options);
        }
        catch (JsonException e)
        {
            error = ReasonForUnreadableLine(line, e.Path);
            return false;
        }

        if (parsed == null || parsed.StationName == null)
        {
            error = StationNameMissing;
            return false;
        }

        if (parsed.Temperature == null)
        {
            error = TemperatureMissing;
            return false;
        }

        var nameBytes = Encoding.UTF8.GetByteCount(parsed.StationName);
        if (nameBytes == 0 || nameBytes > StationName.MaxUtf8Bytes)
        {
            error = StationNameInvalid;
            return false;
        }

        var temperature = parsed.Temperature.Value;
        if (!Temperature.IsInRange(temperature))
        {
            error = TemperatureOutOfRange;
            return false;
        }

        if (!Temperature.HasOneFractionalDigit(temperature))
        {
            error = TemperatureFractionalDigit;
            return false;
        }

        measurement = new Measurement(parsed.StationName, temperature);
        return true;
    }

    private static string ReasonForUnreadableLine(ReadOnlySpan<byte> line, string? path)
    {
        if (!IsWellFormedJson(line))
        {
            return MalformedJson;
        }

        if (path == "$.stationName")
        {
            return StationNameInvalid;
        }

        if (path == "$.temperature")
        {
            return TemperatureNotANumber;
        }

        return MalformedJson;
    }

    private static bool IsWellFormedJson(ReadOnlySpan<byte> line)
    {
        var reader = new Utf8JsonReader(line);
        try
        {
            while (reader.Read())
            {
            }

            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private sealed record MeasurementLine(string? StationName, decimal? Temperature);
}
