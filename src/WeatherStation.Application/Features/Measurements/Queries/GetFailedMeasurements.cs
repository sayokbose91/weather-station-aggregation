using WeatherStation.Application.Interfaces.Repositories;
using WeatherStation.Contracts;

namespace WeatherStation.Application.Features.Measurements.Queries;

public static class GetFailedMeasurements
{
    public sealed record Query(string? Reason, DateTime? From, DateTime? To, int Page, int PageSize);

    public sealed record Response(PagedResponse<FailedMeasurementDto> Result);

    public sealed class Handler(IMeasurementRepository repository)
    {
        private const int MaxPageSize = 500;

        public async Task<Response> Handle(Query query, CancellationToken ct)
        {
            var page = Math.Max(query.Page, 1);
            var pageSize = Math.Max(query.PageSize, 1);
            pageSize = Math.Min(pageSize, MaxPageSize);

            var failedMeasurements =
                await repository.GetFailedMeasurementsAsync(query.Reason, query.From, query.To, page, pageSize, ct);
            
            var totalCount = await repository.CountFailedMeasurementsAsync(query.Reason, query.From, query.To, ct);

            var items = failedMeasurements
                .Select(f => new FailedMeasurementDto(f.Id, f.Line, f.Reason, f.FailedAt))
                .ToList();

            return new Response(new PagedResponse<FailedMeasurementDto>(items, page, pageSize, totalCount));
        }
    }
}