using WeatherStation.Domain.Models;

namespace WeatherStation.Application.Interfaces.Repositories;

public interface IMeasurementRepository
{
    Task AddFailedMeasurementsAsync(IList<FailedMeasurement> failedMeasurements, CancellationToken cancellationToken);

    Task<IList<FailedMeasurement>> GetFailedMeasurementsAsync(string? reason, DateTime? fromDate, DateTime? toDate, int page, int pageSize, CancellationToken cancellationToken);

    Task<int> CountFailedMeasurementsAsync(string? reason, DateTime? fromDate, DateTime? toDate, CancellationToken cancellationToken);
}
