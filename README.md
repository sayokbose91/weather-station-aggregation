# Weather Station Aggregation

Send it a JSONL file of temperature readings and it sends back the min, mean and max for each station.

## Run

You need .NET 10.

Set the API key first. It has to be at least 16 characters. You can put it in src/WeatherStation.Api/appsettings.json (ApiKey:Value) or in user secrets:

    dotnet user-secrets set "ApiKey:Value" "pick-any-key-16-chars-or-more" --project src/WeatherStation.Api
    dotnet run --project src/WeatherStation.Api

It runs on localhost:5080. If there's no key the app won't start.

For Docker, put the key in appsettings.json and run `cd deploy && docker compose up --build`. It runs on port 8080.

## Calling it

With Postman:

- POST http://localhost:5080/api/v1/measurements/aggregate
- Add the X-Api-Key header.
- Body, binary, then pick any file from samples/.

You can also use the .http file in the Api project.

If you use curl, add `-H "Content-Type: application/x-ndjson"` or you get an empty response.

In:

    {"stationName":"hamburg","temperature":10.0}
    {"stationName":"HAMBURG","temperature":20.0}
    {"stationName":"rio de janeiro","temperature":25.0}

Out:

    {"stationName":"Hamburg","min":10.0,"mean":15.0,"max":20.0}
    {"stationName":"Rio De Janeiro","min":25.0,"mean":25.0,"max":25.0}

Bad lines get skipped. I also save them to a SQLite db with the reason (max 10k per upload), and there's a GET endpoint to look at them:

    GET http://localhost:5080/api/v1/measurements/failed?page=1&pageSize=50

It needs the same X-Api-Key header. Page starts at 1, pageSize defaults to 50 and maxes out at 500. Newest come first.

From and to are optional and filter on when the line failed, in UTC. You can use one or both:

    GET http://localhost:5080/api/v1/measurements/failed?from=2026-11-17T09:00:00Z&to=2026-11-17T10:00:00Z

You get back the lines for that page. totalCount is the number of failed lines that match, not the number of pages, so the number of pages is totalCount / pageSize rounded up:

    {"items":[{"id":1,"line":"{\"stationName\":\"\",\"temperature\":10.0}","reason":"stationName must be a UTF-8 string of 1-100 bytes.","failedAt":"2026-11-17T09:12:44.1234567"}],"page":1,"pageSize":50,"totalCount":1}

## Client

This is a console app that sends rows to the API and prints what comes back. It's only there so I could test the API. It's quick and dirty, and not how I'd build a real client.

Start the API first, then put the same key in src/WeatherStation.Client/appsettings.json (ApiKey) and run it from the repo root:

    dotnet run --project src/WeatherStation.Client

By default it makes up 1M rows from 44 cities and streams them, so nothing gets written to disk. The settings are:

- BaseUrl: where the API is. Default is http://localhost:5080.
- ApiKey: the same key as the API.
- Count: how many rows to make up. Max is 1B.
- MeasurementsFile: send this file instead. Count is ignored then.

Environment variables override appsettings, so you don't have to edit the file every time. In PowerShell:

    $env:Count = "1000000000"
    dotnet run --project src/WeatherStation.Client

    $env:MeasurementsFile = "samples/bad-lines.jsonl"
    dotnet run --project src/WeatherStation.Client

The results go to the console. The "Sending..." and "Done in x s" messages go to stderr, so `> out.jsonl` only gets the results. Press any key to stop it.

## Notes

- "Camel case" in the spec is really **title case**, so rio de janeiro becomes Rio De Janeiro.
- Temperature needs **exactly one decimal**. 12 or 12.34 are rejected.
- The **mean is rounded to one decimal**. Everything uses decimal, not double.
- Stations are **sorted by plain character code (ordinal)**, not language rules, so Ürümqi comes after Zagreb. The upside is the order is the same on every machine. A culture sort can differ between my laptop and a container.
- There's **no upload size limit**. 1B rows is about 45 GB.
- **/health doesn't need a key**.
- The body is **streamed with a PipeReader**, blocks are **processed in parallel** and the totals go in a ConcurrentDictionary, so **memory stays flat**.
- I used **FluentValidation** for the line checks. I know it's slower on really big inputs and custom checks would be faster, but I didn't have time for that.
- Projects are Api, Application (Wolverine CQRS), Domain, Infrastructure (SQLite, failed lines only) and Contracts.

## What would break in prod

- The whole file goes up in **one request**. If the connection drops halfway, you have to send it all again.
- **Nginx or ingress limits** (1 MB body, 60s timeout by default) would block it straight away.
- Results only come back in the response and nothing is saved. If the client disconnects, **the work is lost**.
- The **response only starts once the whole upload is read**. All stations sit in memory until then, and there's no cap on how many.
- **One upload takes every core**, and there's no limit on concurrent uploads.
- Failed lines are saved to a **local SQLite file with fire and forget**. They're lost on a crash or container restart, and it doesn't work with multiple instances.
- There's **one shared API key** with no rotation. It's a plain header, so it needs HTTPS.
- **No metrics**, only logs.
- **ToTitleCase has quirks**, like n'djamena becoming N'djamena and 1st becoming 1St.

## With more time

The biggest change is that **I wouldn't send the file through the API at all**. This is how I'd do it:

1. The client calls the API with its API key and gets back a **short lived upload link** to blob storage (a SAS link, valid for maybe 15 minutes) and a **job id**.
2. The **client uploads straight to blob storage** with that link. The **API never touches the 45 GB**.
3. When the file lands, **blob storage publishes an event** (Event Grid). A worker picks that up, reads the file in chunks and puts the chunks on a **message bus** (Service Bus or Kafka). It's **one message per chunk**, not one per row.
4. Then **a worker on the bus does the aggregating**. It's always listening and does the same thing this code does now: parse, validate and add up per station. It **saves the result for each chunk**. When all chunks are done they **get merged**. That's easy because it's just the min of the mins, the max of the maxes, and the sums and counts added up.
5. The **client checks how it's going**. It can keep calling **GET /jobs/{id}** to see how many chunks are done and get the result when it's finished. If there's a frontend, the API can push progress with **server sent events** instead.

This way **the upload and the processing are separated**. If the upload drops, **blob storage can resume it**. If a worker dies, the **message goes back on the bus** and another worker picks it up. When it gets busy, you just **add more workers**.

Depending on what's needed, once a job is finished I'd also **save the report in a database**. That way there's a record of which files were processed and on which day. Once a job is done, a **GET endpoint can return its details and the station stats** from there, so the result isn't lost if nobody was waiting for it.

Chunks that keep failing go to a **dead letter queue**. From there they can be **retried**, or moved to **cold storage** so someone can look at them later.

Other things I'd do:

- A **rate limit** and a **cap on concurrent uploads**.
- **OpenTelemetry**.
- **Line numbers** on failed lines.
- A **load test**, and **integration tests in CI**.

## Perf

On my laptop 1B rows takes about 3.5 min over HTTP, and memory stays around 300 MB. Bad lines don't slow it down.

The server mostly waits for the upload to come in, so the HTTP stream is the limit, not the parsing.

In a pod it gets one worker per CPU, so a small pod will be slower.

## Refs

The way the upload is read in chunks of whole lines and then processed in parallel is heavily inspired by these two Microsoft docs:

- Stream into blocks of whole lines: https://learn.microsoft.com/en-us/dotnet/standard/io/pipelines
- Blocks in parallel: https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.parallel.foreachasync
