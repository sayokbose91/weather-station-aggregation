using System.Globalization;
using WeatherStation.Domain.Models;

namespace WeatherStation.UnitTests.Domain;

public class StationStatisticsTests
{
    [Theory]
    [InlineData("46.2", 2, "23.1")]
    [InlineData("0.3", 2, "0.2")]
    [InlineData("-0.3", 2, "-0.2")]
    [InlineData("1.0", 3, "0.3")]
    public void Mean_IsRoundedToOneDecimal(string sum, long count, string expected)
    {
        var stats = new StationStatistics { Sum = decimal.Parse(sum, CultureInfo.InvariantCulture), Count = count };

        Assert.Equal(expected, stats.Mean.ToString(CultureInfo.InvariantCulture));
    }
}
