using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WeatherStation.Application.Features.Measurements.Queries;
using WeatherStation.Application.Interfaces;
using WeatherStation.Contracts;

namespace WeatherStation.Api.Controllers.V1;

[ApiController]
[Authorize]
[Route(ApiRoutes.V1.FailedMeasurements)]
public sealed class FailedMeasurementsController(IBusProvider bus) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAsync(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken ct,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var query = new GetFailedMeasurements.Query(from, to, page, pageSize);
        var response = await bus.InvokeAsync<GetFailedMeasurements.Response>(query, ct);

        return Ok(response.Result);
    }
}
