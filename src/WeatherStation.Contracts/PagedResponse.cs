namespace WeatherStation.Contracts;

public sealed record PagedResponse<T>(IList<T> Items, int Page, int PageSize, int TotalCount);
