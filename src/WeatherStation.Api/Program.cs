using WeatherStation.Api.Common.Authentication;
using WeatherStation.Application;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddControllers();
builder.Services.AddApiKeyAuthentication(builder.Configuration);

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await app.RunAsync();
