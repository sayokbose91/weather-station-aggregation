namespace WeatherStation.Domain;

public static class Temperature
{
    public static bool IsValid(decimal value) => value.Scale == 1 && value >= -99.9m && value <= 99.9m;
}
