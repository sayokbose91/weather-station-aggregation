using System.Net;
using System.Net.Http.Headers;
using System.Text.Encodings.Web;
using System.Text.Json;
using WeatherStation.Contracts;

namespace WeatherStation.Client;

internal sealed class JsonlContent<T> : HttpContent
{
    private static readonly JsonSerializerOptions Options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    private readonly IEnumerable<T> _items;

    public JsonlContent(IEnumerable<T> items)
    {
        _items = items;
        Headers.ContentType = new MediaTypeHeaderValue(MediaTypes.Ndjson);
    }

    protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context) =>
        SerializeToStreamAsync(stream, context, CancellationToken.None);

    protected override async Task SerializeToStreamAsync(Stream stream, TransportContext? context, CancellationToken cancellationToken)
    {
        await using var writer = new StreamWriter(stream, leaveOpen: true) { NewLine = "\n" };

        foreach (var item in _items)
        {
            var line = JsonSerializer.Serialize(item, Options);
            await writer.WriteLineAsync(line.AsMemory(), cancellationToken);
        }
    }

    protected override bool TryComputeLength(out long length)
    {
        length = 0;
        return false;
    }
}
