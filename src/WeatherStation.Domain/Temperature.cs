namespace WeatherStation.Domain;

public static class Temperature
{
    public const decimal Min = -99.9m;

    public const decimal Max = 99.9m;

    public static bool IsInRange(decimal value) => value >= Min && value <= Max;

    public static bool HasOneFractionalDigit(decimal value) => value.Scale == 1;
}
