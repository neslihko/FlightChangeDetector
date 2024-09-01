using FlightChangeDetector.Models;
using FlightChangeDetector.Repositories;
using FlightChangeDetector.Strategies;

namespace FlightChangeDetector.Services
{
    public class FlightChangeService : IFlightChangeService
    {
        private readonly IFlightRepository _repository;
        private readonly IEnumerable<IChangeDetectionStrategy> _strategies;

        public FlightChangeService(IFlightRepository repository, IEnumerable<IChangeDetectionStrategy> strategies)
        {
            _repository = repository;
            _strategies = strategies;
        }

        public async Task<IEnumerable<FlightChange>> DetectChanges(DateTime startDate, DateTime endDate, int agencyId)
        {
            var changes = new List<FlightChange>();
            var subscriptions = await _repository.GetSubscriptionsByAgency(agencyId);

            foreach (var subscription in subscriptions)
            {
                var flights = await _repository.GetFlightsByRouteAndDateRange(
                    subscription.OriginCityId,
                    subscription.DestinationCityId,
                    startDate,
                    endDate);

                foreach (var strategy in _strategies)
                {
                    changes.AddRange(strategy.DetectChanges(flights));
                }
            }

            return changes;
        }
    }
}


