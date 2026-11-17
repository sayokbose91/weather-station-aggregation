using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WeatherStation.Api.Common.Http;
using WeatherStation.Application.Features.Measurements.Commands;
using WeatherStation.Application.Interfaces;
using WeatherStation.Contracts;

namespace WeatherStation.Api.Controllers.V1;

[ApiController]
[Authorize]
[Route(ApiRoutes.V1.Aggregate)]
public sealed class MeasurementsController(IBusProvider bus) : ControllerBase
{
    [HttpPost]
    [DisableRequestSizeLimit]
    public async Task<IActionResult> AggregateAsync(CancellationToken ct)
    {
        var result = await bus.InvokeAsync<AggregateMeasurements.Response>(new AggregateMeasurements.Command(Request.Body), ct);

        return new JsonlResult<StationDto>(result.Stations);
    }
}
