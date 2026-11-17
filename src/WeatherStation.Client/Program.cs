using System.Diagnostics;
using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using Refit;
using WeatherStation.Client;
using WeatherStation.Client.Common;
using WeatherStation.Client.Interfaces;
using WeatherStation.Client.Models;
using WeatherStation.Contracts;

Console.OutputEncoding = System.Text.Encoding.UTF8;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables()
    .Build();

var count = Math.Min(configuration.GetValue<long>("Count"), Constants.MaxMeasurements);
var file = configuration["MeasurementsFile"];

using var http = new HttpClient { BaseAddress = new Uri(configuration["BaseUrl"]!), Timeout = Timeout.InfiniteTimeSpan };
http.DefaultRequestHeaders.Add(ApiRoutes.ApiKeyHeader, configuration["ApiKey"]);
http.DefaultRequestHeaders.ExpectContinue = true;

var api = RestService.For<IWeatherStationApi>(http);

if (string.IsNullOrEmpty(file))
{
    Console.Error.WriteLine($"Sending {count:N0} generated measurements.");
}
else
{
    Console.Error.WriteLine($"Sending {file}.");
}

using var cancellation = new CancellationTokenSource();
if (!Console.IsInputRedirected)
{
    Console.Error.WriteLine("Press any key to stop.");
    _ = Task.Run(() =>
    {
        Console.ReadKey(intercept: true);
        cancellation.Cancel();
    });
}

var stopwatch = Stopwatch.StartNew();

try
{
    using var measurements = CreateContent(file, count);
    using var results = new StreamReader(await api.AggregateAsync(measurements, cancellation.Token));

    string? line;
    while ((line = await results.ReadLineAsync(cancellation.Token)) != null)
    {
        Console.WriteLine(line);
    }
}
catch (ApiException e)
{
    Console.Error.WriteLine($"{(int)e.StatusCode} {e.ReasonPhrase}: {e.Content}");
    return 1;
}
catch (ApiRequestException e)
{
    Console.Error.WriteLine(e.Message);
    return 1;
}
catch (OperationCanceledException)
{
    Console.Error.WriteLine("Stopped.");
    return 1;
}

Console.Error.WriteLine($"Done in {stopwatch.Elapsed.TotalSeconds:N1} s");
return 0;

static HttpContent CreateContent(string? file, long count)
{
    if (string.IsNullOrEmpty(file))
    {
        return new JsonlContent<Measurement>(MeasurementGenerator.Generate(count));
    }

    var content = new StreamContent(File.OpenRead(file));
    content.Headers.ContentType = new MediaTypeHeaderValue(MediaTypes.Ndjson);
    return content;
}
