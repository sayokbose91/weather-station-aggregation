using System.Text.Json;
using FluentValidation;
using WeatherStation.Domain;

namespace WeatherStation.Application.Features.Measurements.Parsing;

internal sealed class MeasurementLineParser(IValidator<MeasurementLine> validator)
{
    internal const string MalformedJson = "Malformed JSON.";

    private static readonly JsonSerializerOptions Options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public bool TryParse(string line, out Measurement measurement, out string error)
    {
        measurement = default;
        error = string.Empty;

        MeasurementLine? parsed;
        try
        {
            parsed = JsonSerializer.Deserialize<MeasurementLine>(line, Options);
        }
        catch (JsonException)
        {
            error = MalformedJson;
            return false;
        }

        if (parsed == null)
        {
            error = MalformedJson;
            return false;
        }

        var result = validator.Validate(parsed);
        if (!result.IsValid)
        {
            error = result.Errors[0].ErrorMessage;
            return false;
        }

        measurement = new Measurement(parsed.StationName, parsed.Temperature!.Value);
        return true;
    }
}
