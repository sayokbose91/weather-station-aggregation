using System.Buffers;
using System.IO.Pipelines;
using WeatherStation.Application.Features.Measurements.Parsing;
using WeatherStation.Domain;

namespace WeatherStation.Application.Features.Measurements;

internal sealed class MeasurementAggregator : IMeasurementAggregator
{
    internal const int MaxLineBytes = 64 * 1024;

    private const int ReadSizeBytes = 1024 * 1024;

    public async Task<IReadOnlyDictionary<string, StationStatistics>> AggregateAsync(Stream input, CancellationToken cancellationToken)
    {
        var stations = new Dictionary<string, StationStatistics>();
        var reader = PipeReader.Create(input, new StreamPipeReaderOptions(bufferSize: ReadSizeBytes, leaveOpen: true));

        try
        {
            while (true)
            {
                var result = await reader.ReadAsync(cancellationToken);
                var lines = new SequenceReader<byte>(result.Buffer);

                while (lines.TryReadTo(out ReadOnlySpan<byte> line, (byte)'\n'))
                {
                    AddLine(stations, line);
                }

                if (lines.Remaining > MaxLineBytes) lines.AdvanceToEnd();

                if (result.IsCompleted)
                {
                    AddLine(stations, lines.UnreadSequence.ToArray());
                    break;
                }

                reader.AdvanceTo(lines.Position, result.Buffer.End);
            }
        }
        finally
        {
            await reader.CompleteAsync();
        }

        return stations;
    }

    private static void AddLine(Dictionary<string, StationStatistics> stations, ReadOnlySpan<byte> line)
    {
        line = line.TrimEnd((byte)'\r');
        if (line.IsEmpty)
        {
            return;
        }

        if (!MeasurementLineParser.TryParse(line, out var measurement, out _)) return;

        var key = StationName.ToKey(measurement.StationName);
        if (!stations.TryGetValue(key, out var stats))
        {
            stats = new StationStatistics();
            stations[key] = stats;
        }

        Totals.Add(stats, measurement.Temperature);
    }
}
