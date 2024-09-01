using FlightChangeDetector.Models;

namespace FlightChangeDetector.Repositories
{
    public interface IFlightRepository
    {
        Task<IEnumerable<Flight>> GetFlightsByRouteAndDateRange(int originCityId, int destinationCityId, DateTime startDate, DateTime endDate);
        Task<IEnumerable<Subscription>> GetSubscriptionsByAgency(int agencyId);
    }
}

