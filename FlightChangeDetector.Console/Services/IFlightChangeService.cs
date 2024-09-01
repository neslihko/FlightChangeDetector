using FlightChangeDetector.Models;

namespace FlightChangeDetector.Services
{
    public interface IFlightChangeService
    {
        Task<IEnumerable<FlightChange>> DetectChanges(DateTime startDate, DateTime endDate, int agencyId);
    }
}
