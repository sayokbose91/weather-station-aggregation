using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using WeatherStation.Contracts;

namespace WeatherStation.Api.Common.Http;

internal sealed class JsonlResult<T>(IEnumerable<T> items) : IActionResult
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    private static readonly byte[] NewLine = new[] { (byte)'\n' };

    public async Task ExecuteResultAsync(ActionContext context)
    {
        var response = context.HttpContext.Response;
        var cancellationToken = context.HttpContext.RequestAborted;
        response.ContentType = MediaTypes.Ndjson;

        foreach (var item in items)
        {
            await JsonSerializer.SerializeAsync(response.Body, item, Options, cancellationToken);
            await response.Body.WriteAsync(NewLine, cancellationToken);
        }
    }
}
