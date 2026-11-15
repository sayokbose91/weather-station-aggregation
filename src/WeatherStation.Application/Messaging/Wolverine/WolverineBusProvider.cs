using WeatherStation.Application.Interfaces;
using Wolverine;

namespace WeatherStation.Application.Messaging.Wolverine;

internal sealed class WolverineBusProvider(IMessageBus bus) : IBusProvider
{
    public Task<TResponse> InvokeAsync<TResponse>(object request, CancellationToken cancellationToken = default) =>
        bus.InvokeAsync<TResponse>(request, cancellationToken);
}
