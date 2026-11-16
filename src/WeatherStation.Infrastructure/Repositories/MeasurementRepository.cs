using Microsoft.EntityFrameworkCore;
using WeatherStation.Application.Interfaces.Repositories;
using WeatherStation.Domain.Models;
using WeatherStation.Infrastructure.Contexts;

namespace WeatherStation.Infrastructure.Repositories;

public class MeasurementRepository(WeatherStationDbContext context) : IMeasurementRepository
{
    public async Task AddFailedMeasurementsAsync(IList<FailedMeasurement> failedMeasurements, CancellationToken cancellationToken)
    {
        await context.FailedMeasurements.AddRangeAsync(failedMeasurements, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IList<FailedMeasurement>> GetFailedMeasurementsAsync(string? reason, DateTime? fromDate, DateTime? toDate, int page, int pageSize, CancellationToken cancellationToken)
    {
        return await Filter(reason, fromDate, toDate)
            .OrderByDescending(f => f.FailedAt)
            .ThenByDescending(f => f.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountFailedMeasurementsAsync(string? reason, DateTime? fromDate, DateTime? toDate, CancellationToken cancellationToken)
    {
        return await Filter(reason, fromDate, toDate).CountAsync(cancellationToken);
    }

    private IQueryable<FailedMeasurement> Filter(string? reason, DateTime? fromDate, DateTime? toDate)
    {
        var query = context.FailedMeasurements.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(reason))
        {
            query = query.Where(f => EF.Functions.Like(f.Reason, "%" + reason + "%"));
        }

        if (fromDate != null)
        {
            query = query.Where(f => f.FailedAt >= fromDate.Value);
        }

        if (toDate != null)
        {
            query = query.Where(f => f.FailedAt <= toDate.Value);
        }

        return query;
    }
}
