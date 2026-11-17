using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using WeatherStation.Application.Features.Measurements;
using WeatherStation.Application.Features.Measurements.Commands;
using WeatherStation.Application.Features.Measurements.Parsing;
using WeatherStation.Application.Features.Measurements.Validation;
using WeatherStation.Application.Interfaces.Repositories;
using WeatherStation.Contracts;
using WeatherStation.Domain.Models;

namespace WeatherStation.UnitTests.Application;

public class AggregateMeasurementsTests
{
    [Fact]
    public async Task Handle_SpecExample()
    {
        var input =
            "{\"stationName\":\"Hamburg\",\"temperature\":12.0}\n" +
            "{\"stationName\":\"Bulawayo\",\"temperature\":8.9}\n" +
            "{\"stationName\":\"hamburg\",\"temperature\":34.2}\n";

        var response = await HandleAsync(input, new FakeFailedMeasurementRepository());

        Assert.Equal(
            [
                new StationDto("Bulawayo", 8.9m, 8.9m, 8.9m),
                new StationDto("Hamburg", 12.0m, 23.1m, 34.2m),
            ],
            response.Stations);
    }

    [Fact]
    public async Task Handle_BadLines_AreSkippedAndSaved()
    {
        var input =
            "{\"stationName\":\"Hamburg\",\"temperature\":12.0}\n" +
            "{\"stationName\":\"Hamburg\",\"temperature\":12}\n" +
            "not json\n";
        var repository = new FakeFailedMeasurementRepository();

        var response = await HandleAsync(input, repository);

        Assert.Equal([new StationDto("Hamburg", 12.0m, 12.0m, 12.0m)], response.Stations);
        Assert.Equal(2, repository.Saved.Count);
    }

    private static Task<AggregateMeasurements.Response> HandleAsync(string input, FakeFailedMeasurementRepository repository)
    {
        var aggregator = new MeasurementAggregator(new MeasurementLineParser(new MeasurementLineValidator()));
        var handler = new AggregateMeasurements.Handler(aggregator, repository, NullLogger<AggregateMeasurements.Handler>.Instance);
        return handler.Handle(new AggregateMeasurements.Command(new MemoryStream(Encoding.UTF8.GetBytes(input))), CancellationToken.None);
    }

    private sealed class FakeFailedMeasurementRepository : IFailedMeasurementRepository
    {
        public List<FailedMeasurement> Saved { get; } = [];

        public Task AddFailedMeasurementsAsync(IList<FailedMeasurement> failedMeasurements, CancellationToken cancellationToken)
        {
            Saved.AddRange(failedMeasurements);
            return Task.CompletedTask;
        }

        public Task<IList<FailedMeasurement>> GetFailedMeasurementsAsync(DateTime? fromDate, DateTime? toDate, int page, int pageSize, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<int> CountFailedMeasurementsAsync(DateTime? fromDate, DateTime? toDate, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
