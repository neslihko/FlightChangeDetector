using FlightChangeDetector.Models;

namespace FlightChangeDetector.Strategies
{
    public interface IChangeDetectionStrategy
    {
        IEnumerable<FlightChange> DetectChanges(IEnumerable<Flight> flights);
    }
}
