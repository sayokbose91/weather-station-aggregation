using WeatherStation.Domain.Models;

namespace WeatherStation.Application.Interfaces.Repositories;

public interface IFailedMeasurementRepository
{
    Task AddFailedMeasurementsAsync(IList<FailedMeasurement> failedMeasurements, CancellationToken cancellationToken);

    Task<IList<FailedMeasurement>> GetFailedMeasurementsAsync(DateTime? fromDate, DateTime? toDate, int page, int pageSize, CancellationToken cancellationToken);

    Task<int> CountFailedMeasurementsAsync(DateTime? fromDate, DateTime? toDate, CancellationToken cancellationToken);
}
