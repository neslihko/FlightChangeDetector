using FlightChangeDetector.Models;

namespace FlightChangeDetector.Strategies
{
    public class NewFlightDetectionStrategy : IChangeDetectionStrategy
    {

        public IEnumerable<FlightChange> DetectChanges(IEnumerable<Flight> flights)
        {
            return flights.Where(flight =>
                !flights.Any(f =>
                    f.AirlineId == flight.AirlineId &&
                    Math.Abs((f.DepartureTime - flight.DepartureTime.AddDays(-7)).TotalMinutes) <= 30
                )
            ).Select(f => new FlightChange(f, "New"));
        }
    }

}
