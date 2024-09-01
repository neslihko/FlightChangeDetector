using FlightChangeDetector.Models;
using Microsoft.EntityFrameworkCore;

namespace FlightChangeDetector.Data
{
    public interface IFlightDbContext
    {
        DbSet<Route> Routes { get; set; }
        DbSet<Flight> Flights { get; set; }
        DbSet<Subscription> Subscriptions { get; set; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }

    public class FlightDbContext : DbContext, IFlightDbContext
    {
        public DbSet<Route> Routes { get; set; }
        public DbSet<Flight> Flights { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }

        public FlightDbContext(DbContextOptions<FlightDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            _ = modelBuilder.Entity<Route>().HasKey(r => r.RouteId);
            _ = modelBuilder.Entity<Flight>().HasKey(f => f.FlightId);
            _ = modelBuilder.Entity<Subscription>().HasKey(s => new { s.AgencyId, s.OriginCityId, s.DestinationCityId });
        }
    }
}