FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish src/WeatherStation.Api/WeatherStation.Api.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
ENV ConnectionStrings__WeatherStation="Data Source=/tmp/weather-station.db"
EXPOSE 8080
ENTRYPOINT ["dotnet", "WeatherStation.Api.dll"]
