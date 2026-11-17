using WeatherStation.Domain;

namespace WeatherStation.UnitTests.Domain;

public class StationNameTests
{
    [Theory]
    [InlineData("hamburg", "Hamburg")]
    [InlineData("NEW YORK", "New York")]
    [InlineData("st. john's", "St. John's")]
    [InlineData("rio de janeiro", "Rio De Janeiro")]
    [InlineData("cluj-napoca", "Cluj-Napoca")]
    public void ToDisplayName_IsTitleCase(string input, string expected)
    {
        Assert.Equal(expected, StationName.ToDisplayName(input));
    }

    [Theory]
    [InlineData("hamburg")]
    [InlineData("HAMBURG")]
    [InlineData("Hamburg")]
    public void ToKey_CaseVariants_AreOneStation(string input)
    {
        Assert.Equal("HAMBURG", StationName.ToKey(input));
    }
}
