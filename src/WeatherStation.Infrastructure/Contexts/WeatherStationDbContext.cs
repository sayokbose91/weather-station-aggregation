using Microsoft.EntityFrameworkCore;
using WeatherStation.Domain.Models;

namespace WeatherStation.Infrastructure.Contexts;

public class WeatherStationDbContext(DbContextOptions<WeatherStationDbContext> options) : DbContext(options)
{
    public virtual DbSet<FailedMeasurement> FailedMeasurements { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FailedMeasurement>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.FailedAt);
        });
    }
}
