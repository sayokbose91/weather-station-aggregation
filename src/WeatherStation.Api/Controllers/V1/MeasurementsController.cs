using Microsoft.AspNetCore.Mvc;
using WeatherStation.Api.Common.Http;
using WeatherStation.Application.Features.Measurements;
using WeatherStation.Contracts;
using WeatherStation.Domain;

namespace WeatherStation.Api.Controllers.V1;

[ApiController]
[Route(ApiRoutes.V1.Aggregate)]
public sealed class MeasurementsController(IMeasurementAggregator aggregator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> AggregateAsync(CancellationToken ct)
    {
        var totals = await aggregator.AggregateAsync(Request.Body, ct);

        var stations = totals
            .Select(t => new StationDto(StationName.ToDisplayName(t.Key), t.Value.Min, t.Value.Mean, t.Value.Max))
            .OrderBy(s => s.StationName, StringComparer.Ordinal)
            .ToList();

        return new JsonlResult<StationDto>(stations.ToAsyncEnumerable());
    }
}
