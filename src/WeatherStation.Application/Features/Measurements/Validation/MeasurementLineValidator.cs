using System.Text;
using FluentValidation;
using WeatherStation.Application.Features.Measurements.Parsing;
using WeatherStation.Domain;

namespace WeatherStation.Application.Features.Measurements.Validation;

internal sealed class MeasurementLineValidator : AbstractValidator<MeasurementLine>
{
    internal const string StationNameMissing = "Missing stationName.";
    internal const string StationNameInvalid = "stationName must be a UTF-8 string of 1-100 bytes.";
    internal const string TemperatureMissing = "Missing temperature.";
    internal const string TemperatureOutOfRange = "temperature must be between -99.9 and 99.9.";
    internal const string TemperatureFractionalDigit = "temperature must have one fractional digit.";

    public MeasurementLineValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(m => m.StationName)
            .NotNull().WithMessage(StationNameMissing)
            .Must(BeValidStationName).WithMessage(StationNameInvalid);

        RuleFor(m => m.Temperature)
            .NotNull().WithMessage(TemperatureMissing)
            .InclusiveBetween(Temperature.Min, Temperature.Max).WithMessage(TemperatureOutOfRange)
            .Must(t => Temperature.HasOneFractionalDigit(t!.Value)).WithMessage(TemperatureFractionalDigit);
    }

    private static bool BeValidStationName(string name)
    {
        var bytes = Encoding.UTF8.GetByteCount(name);
        if (bytes == 0)
        {
            return false;
        }

        if (bytes > StationName.MaxUtf8Bytes)
        {
            return false;
        }

        return true;
    }
}
