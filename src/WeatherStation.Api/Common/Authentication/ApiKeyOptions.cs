namespace WeatherStation.Api.Common.Authentication;

public sealed class ApiKeyOptions
{
    public const string SectionName = "ApiKey";

    public string Value { get; init; } = string.Empty;
}
