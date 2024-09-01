using FlightChangeDetector.Data;
using FlightChangeDetector.Models;
using Microsoft.EntityFrameworkCore;

namespace FlightChangeDetector.Repositories
{
    public class FlightRepository : IFlightRepository
    {
        private readonly FlightDbContext _context;

        public FlightRepository(FlightDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Flight>> GetFlightsByRouteAndDateRange(int originCityId, int destinationCityId, DateTime startDate, DateTime endDate)
        {
            return await _context.Flights
                .Include(f => f.Route)
                .Where(f => f.Route.OriginCityId == originCityId &&
                    f.Route.DestinationCityId == destinationCityId &&
                    f.DepartureTime >= startDate &&
                    f.DepartureTime <= endDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Subscription>> GetSubscriptionsByAgency(int agencyId)
        {
            return await _context.Subscriptions
                .Where(s => s.AgencyId == agencyId)
                .ToListAsync();
        }
    }
}
