using WeatherStation.Api;
using WeatherStation.Application;
using WeatherStation.Contracts;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddApi(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks($"/{ApiRoutes.Health}");

await app.RunAsync();
