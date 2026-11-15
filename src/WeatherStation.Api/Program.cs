using WeatherStation.Application;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();

await app.RunAsync();
