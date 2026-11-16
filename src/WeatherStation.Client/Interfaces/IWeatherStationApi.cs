using Refit;
using WeatherStation.Contracts;

namespace WeatherStation.Client.Interfaces;

public interface IWeatherStationApi
{
    [Post("/" + ApiRoutes.V1.Aggregate)]
    Task<Stream> AggregateAsync([Body] HttpContent measurements, CancellationToken cancellationToken);
}
