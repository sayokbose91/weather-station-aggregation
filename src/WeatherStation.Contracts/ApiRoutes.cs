namespace WeatherStation.Contracts;

public static class ApiRoutes
{
    public const string Health = "health";
    public const string ApiKeyHeader = "X-Api-Key";

    public static class V1
    {
        public const string Aggregate = "api/v1/measurements/aggregate";
        public const string FailedMeasurements = "api/v1/measurements/failed";
    }
}
