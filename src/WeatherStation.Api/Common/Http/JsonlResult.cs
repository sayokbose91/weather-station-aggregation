using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using WeatherStation.Contracts;

namespace WeatherStation.Api.Common.Http;

internal sealed class JsonlResult<T>(IAsyncEnumerable<T> items) : IActionResult
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    private static readonly byte[] NewLine = "\n"u8.ToArray();

    public async Task ExecuteResultAsync(ActionContext context)
    {
        var response = context.HttpContext.Response;
        var cancellationToken = context.HttpContext.RequestAborted;
        response.ContentType = MediaTypes.Ndjson;

        await foreach (var item in items.WithCancellation(cancellationToken))
        {
            await JsonSerializer.SerializeAsync(response.Body, item, Options, cancellationToken);
            await response.Body.WriteAsync(NewLine, cancellationToken);
        }
    }
}
