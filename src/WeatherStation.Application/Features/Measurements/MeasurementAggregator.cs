using System.Buffers;
using System.Collections.Concurrent;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Text;

using WeatherStation.Application.Features.Measurements.Parsing;
using WeatherStation.Domain;
using WeatherStation.Domain.Models;

namespace WeatherStation.Application.Features.Measurements;

internal sealed class MeasurementAggregator(MeasurementLineParser parser) : IMeasurementAggregator
{
    private const int BlockSizeBytes = 64 * 1024;

    internal const int MaxLineBytes = 64 * 1024;

    private const int MaxFailedMeasurements = 10_000;

    private const int MaxStoredLineLength = 1_000;

    public async Task<AggregationResult> AggregateAsync(Stream input, CancellationToken cancellationToken)
    {
        var stations = new ConcurrentDictionary<string, StationStatistics>();
        var failedMeasurements = new ConcurrentQueue<FailedMeasurement>();

        await Parallel.ForEachAsync(ReadBlocksAsync(input, cancellationToken), cancellationToken, (block, _) =>
        {
            var text = Encoding.UTF8.GetString(block.Span);
            foreach (var line in text.Split('\n'))
            {
                AddLine(stations, failedMeasurements, line);
            }

            return ValueTask.CompletedTask;
        });

        return new AggregationResult(stations, failedMeasurements.ToList());
    }

    private static async IAsyncEnumerable<ReadOnlyMemory<byte>> ReadBlocksAsync(
        Stream input,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var reader = PipeReader.Create(input, new StreamPipeReaderOptions(bufferSize: BlockSizeBytes, leaveOpen: true));

        try
        {
            while (true)
            {
                var result = await reader.ReadAsync(cancellationToken);
                var bytes = result.Buffer.ToArray();

                var end = result.IsCompleted ? bytes.Length : Array.LastIndexOf(bytes, (byte)'\n') + 1;
                if (end > 0) yield return bytes.AsMemory(0, end);

                if (result.IsCompleted) break;

                if (bytes.Length - end > MaxLineBytes) end = bytes.Length;

                reader.AdvanceTo(result.Buffer.GetPosition(end), result.Buffer.End);
            }
        }
        finally
        {
            await reader.CompleteAsync();
        }
    }

    private void AddLine(
        ConcurrentDictionary<string, StationStatistics> stations,
        ConcurrentQueue<FailedMeasurement> failedMeasurements,
        string line)
    {
        line = line.TrimEnd('\r');
        if (line.Length == 0)
        {
            return;
        }

        if (!parser.TryParse(line, out var measurement, out var error))
        {
            AddFailedMeasurement(failedMeasurements, line, error);
            return;
        }

        var stats = stations.GetOrAdd(StationName.ToKey(measurement.StationName), _ => new StationStatistics());
        lock (stats)
        {
            StationStatisticsCalculator.Add(stats, measurement.Temperature);
        }
    }

    private static void AddFailedMeasurement(
        ConcurrentQueue<FailedMeasurement> failedMeasurements,
        string line,
        string reason)
    {
        if (failedMeasurements.Count >= MaxFailedMeasurements)
        {
            return;
        }

        var text = line;
        if (text.Length > MaxStoredLineLength)
        {
            text = text.Substring(0, MaxStoredLineLength);
        }

        failedMeasurements.Enqueue(new FailedMeasurement
        {
            Line = text,
            Reason = reason,
            FailedAt = DateTime.UtcNow,
        });
    }
}