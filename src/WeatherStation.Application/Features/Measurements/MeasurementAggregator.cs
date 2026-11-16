using System.Buffers;
using System.Collections.Concurrent;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using WeatherStation.Application.Features.Measurements.Parsing;
using WeatherStation.Domain;

namespace WeatherStation.Application.Features.Measurements;

internal sealed class MeasurementAggregator : IMeasurementAggregator
{
    private const int BlockSizeBytes = 64 * 1024;

    internal const int MaxLineBytes = 64 * 1024;

    public async Task<IDictionary<string, StationStatistics>> AggregateAsync(Stream input, CancellationToken cancellationToken)
    {
        var stations = new ConcurrentDictionary<string, StationStatistics>();

        await Parallel.ForEachAsync(ReadBlocksAsync(input, cancellationToken), cancellationToken, (block, _) =>
        {
            var lines = block.Span;
            foreach (var range in lines.Split((byte)'\n'))
            {
                AddLine(stations, lines[range]);
            }

            return ValueTask.CompletedTask;
        });

        return stations;
    }

    private static async IAsyncEnumerable<ReadOnlyMemory<byte>> ReadBlocksAsync(Stream input, [EnumeratorCancellation] CancellationToken cancellationToken)
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

    private static void AddLine(ConcurrentDictionary<string, StationStatistics> stations, ReadOnlySpan<byte> line)
    {
        line = line.TrimEnd((byte)'\r');
        if (line.IsEmpty)
        {
            return;
        }

        if (!MeasurementLineParser.TryParse(line, out var measurement, out _)) return;

        var stats = stations.GetOrAdd(StationName.ToKey(measurement.StationName), _ => new StationStatistics());
        lock (stats)
        {
            Totals.Add(stats, measurement.Temperature);
        }
    }
}
