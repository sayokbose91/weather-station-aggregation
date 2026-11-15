namespace WeatherStation.Application.Interfaces;

public interface IBusProvider
{
    Task<TResponse> InvokeAsync<TResponse>(object request, CancellationToken cancellationToken = default);
}
