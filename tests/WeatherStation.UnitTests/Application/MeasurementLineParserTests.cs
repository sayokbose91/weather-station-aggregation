using WeatherStation.Application.Features.Measurements.Parsing;
using WeatherStation.Application.Features.Measurements.Validation;

namespace WeatherStation.UnitTests.Application;

public class MeasurementLineParserTests
{
    private static readonly MeasurementLineParser Parser = new(new MeasurementLineValidator());

    [Theory]
    [InlineData("not json", MeasurementLineParser.MalformedJson)]
    [InlineData("{\"temperature\":12.0}", MeasurementLineValidator.StationNameMissing)]
    [InlineData("{\"stationName\":\"Hamburg\"}", MeasurementLineValidator.TemperatureMissing)]
    [InlineData("{\"stationName\":\"\",\"temperature\":12.0}", MeasurementLineValidator.StationNameInvalid)]
    [InlineData("{\"stationName\":\"Hamburg\",\"temperature\":100.0}", MeasurementLineValidator.TemperatureOutOfRange)]
    [InlineData("{\"stationName\":\"Hamburg\",\"temperature\":12.34}", MeasurementLineValidator.TemperatureFractionalDigit)]
    [InlineData("{\"stationName\":\"Hamburg\",\"temperature\":12}", MeasurementLineValidator.TemperatureFractionalDigit)]
    public void TryParse_BadLine_GivesTheSpecReason(string line, string reason)
    {
        var ok = Parser.TryParse(line, out _, out var error);

        Assert.False(ok);
        Assert.Equal(reason, error);
    }
}
