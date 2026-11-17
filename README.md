# Weather Station Aggregation

Send it a JSONL file of temperature readings and it sends back the min, mean and max for each station.

## Getting started

You need .NET 10. Docker is optional.

### 1. Clone it

    git clone https://github.com/sayokbose91/weather-station-aggregation.git
    cd weather-station-aggregation

Run everything below from this folder.

### 2. Set the API key

Pick any key, it just can't be empty. User secrets is easiest:

    dotnet user-secrets set "ApiKey:Value" "your-api-key" --project src/WeatherStation.Api

Or put it in src/WeatherStation.Api/appsettings.json under ApiKey:Value.

If there's no key the API won't start and tells you to set one.

### 3. Start the API

    dotnet run --project src/WeatherStation.Api

It's on http://localhost:5080. Open http://localhost:5080/health and you should see Healthy.

With Docker instead:

1. Put the key in src/WeatherStation.Api/appsettings.json under ApiKey:Value. User secrets don't get into the container.
2. Run `cd deploy && docker compose up --build`.
3. It's on http://localhost:8080 this time.

Every call except /health needs the same key in the X-Api-Key header.

### 4. Test it with the API

The easiest way is the collection in postman/, it works in Postman and Insomnia too.

1. Import postman/WeatherStation.postman_collection.json.
2. Put the same key in the apiKey variable. In Postman it's the Variables tab on the collection, in Insomnia it's the Base Environment (Ctrl+E).
3. On Docker? Change baseUrl to http://localhost:8080.
4. Hit Spec example, you should get Bulawayo and Hamburg back.
5. Hit Bad lines and then Latest page, that shows the lines it skipped and why.

Or by hand in Postman:

- POST http://localhost:5080/api/v1/measurements/aggregate
- Add the X-Api-Key header.
- Body, binary, then pick any file from samples/.

You can also use the .http file in the Api project.

If you use curl, add `-H "Content-Type: application/x-ndjson"` or you get an empty response:

    curl -X POST http://localhost:5080/api/v1/measurements/aggregate -H "X-Api-Key: your-api-key" -H "Content-Type: application/x-ndjson" --data-binary @samples/spec-station-names.jsonl

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

### 5. Test it with the console app

This is a console app that sends rows to the API and prints what comes back. It's only there so I could test the API with lots of data. It's quick and dirty, and not how I'd build a real client.

1. Keep the API running.
2. Put the same key in src/WeatherStation.Client/appsettings.json under ApiKey.
3. In a second terminal, run:

       dotnet run --project src/WeatherStation.Client

By default it makes up 1M rows from 44 cities and streams them, so nothing gets written to disk. The settings are:

- BaseUrl: where the API is. Default is http://localhost:5080. Use http://localhost:8080 for Docker.
- ApiKey: the same key as the API.
- Count: how many rows to make up. Max is 1B.
- MeasurementsFile: send this file instead. Count is ignored then.

Environment variables override appsettings, so you don't have to edit the file every time. In PowerShell:

    $env:Count = "1000000000"
    dotnet run --project src/WeatherStation.Client

    $env:MeasurementsFile = "samples/bad-lines.jsonl"
    dotnet run --project src/WeatherStation.Client

The results go to the console. The "Sending..." and "Done in x s" messages go to stderr, so `> out.jsonl` only gets the results. Press any key to stop it.

### 6. Run the tests

    dotnet test

## Notes

- "Camel case" in the spec is really **title case**, so rio de janeiro becomes Rio De Janeiro.
- Temperature needs **exactly one decimal**. 12 or 12.34 are rejected.
- The **mean is rounded to one decimal**. Everything uses decimal, not double.
- Stations are **sorted by plain character code (ordinal)**, not language rules, so Ürümqi comes after Zagreb. The upside is the order is the same on every machine. A culture sort can differ between my laptop and a container.
- There's **no upload size limit**. 1B rows is about **45-47 GB** with a normal mix of city names. It depends on the station names and temperatures, because every row is a full JSON line. {"stationName":"Hamburg","temperature":12.0} is 46 bytes with the newline, Rome is a bit shorter and Las Palmas de Gran Canaria is a lot longer. My 1B test file with 44 cities averaged 47 bytes a row, so 47 GB.
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
- Failed lines are saved to a **local SQLite file**. They're lost on a container restart, and it doesn't work with multiple instances.
- There's **one shared API key** with no rotation. It's a plain header, so it needs HTTPS.
- **No metrics**, only logs.
- **ToTitleCase has quirks**, like n'djamena becoming N'djamena and 1st becoming 1St.

## With more time

The biggest change is that **I wouldn't send the file through the API at all**. This is how I'd do it:

1. The client calls the API with its API key and gets back a **short lived upload link** to blob storage (a SAS link, valid for maybe 15 minutes) and a **job id**.
2. The **client uploads straight to blob storage** with that link. The **API never touches the 45-47 GB**.
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

I ran these on my laptop (22 cores, Release build).

A 1B row file (47 GB) takes 4.8 min with no HTTP and memory peaks at 184 MB. No HTTP means the same processing code reading straight from the file, so I could see what HTTP costs.

A 10 GB file (212M rows) takes 67 s with no HTTP and 64 s over HTTP. Memory peaks at 179 MB and 354 MB.

1B rows made up by the client and sent over HTTP takes 12.2 min, memory peaks at 339 MB.

So it's about 3.5M rows a second, and memory stays flat no matter how big the file is.

HTTP adds basically nothing, the 10 GB file takes the same time with or without it. The limit is parsing and validating each line.

The 12 min run is slow because of the client. It can only make up about 1.4M rows a second, and the server was only using about 3 of the 22 cores.

In a pod it gets one worker per CPU, so a small pod will be slower.

## Refs

The way the upload is read in chunks of whole lines and then processed in parallel is heavily inspired by these two Microsoft docs:

- Stream into blocks of whole lines: https://learn.microsoft.com/en-us/dotnet/standard/io/pipelines
- Blocks in parallel: https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.parallel.foreachasync

The API key check is the standard AuthenticationHandler pattern from this:

- The ASP.NET Core team's sample handler: https://github.com/dotnet/aspnetcore/blob/main/src/Security/samples/CustomAuthorizationFailureResponse/Authentication/SampleAuthenticationHandler.cs
