using FlightChangeDetector.Models;
using FlightChangeDetector.Repositories;
using FlightChangeDetector.Services;
using FlightChangeDetector.Strategies;
using Moq;

namespace FlightChangeDetector.Tests
{
    [TestFixture]
    public class FlightChangeServiceTests
    {
        private Mock<IFlightRepository> _mockRepository;
        private List<IChangeDetectionStrategy> _strategies;
        private FlightChangeService _service;

        [SetUp]
        public void Setup()
        {
            _mockRepository = new Mock<IFlightRepository>();
            _strategies = new List<IChangeDetectionStrategy>
            {
                new NewFlightDetectionStrategy(),
                new DiscontinuedFlightDetectionStrategy()
            };
            _service = new FlightChangeService(_mockRepository.Object, _strategies);
        }

        [Test]
        public async Task DetectChanges_ShouldReturnCorrectChanges()
        {
            // Arrange
            var startDate = new DateTime(2023, 1, 1);
            var endDate = new DateTime(2023, 1, 31);
            var agencyId = 1;

            var route = new Route { RouteId = 1, OriginCityId = 1, DestinationCityId = 2 };
            var subscription = new Subscription { AgencyId = agencyId, OriginCityId = 1, DestinationCityId = 2 };
            var flights = new List<Flight>
            {
                new Flight { FlightId = 1, RouteId = 1, AirlineId = 1, DepartureTime = new DateTime(2023, 1, 1), ArrivalTime = new DateTime(2023, 1, 1, 2, 0, 0), Route = route },
                new Flight { FlightId = 2, RouteId = 1, AirlineId = 1, DepartureTime = new DateTime(2023, 1, 8), ArrivalTime = new DateTime(2023, 1, 8, 2, 0, 0), Route = route },
                new Flight { FlightId = 3, RouteId = 1, AirlineId = 1, DepartureTime = new DateTime(2023, 1, 15), ArrivalTime = new DateTime(2023, 1, 15, 2, 0, 0), Route = route },
            };

            _ = _mockRepository.Setup(r => r.GetSubscriptionsByAgency(agencyId)).ReturnsAsync(new List<Subscription> { subscription });
            _ = _mockRepository.Setup(r => r.GetFlightsByRouteAndDateRange(subscription.OriginCityId, subscription.DestinationCityId, startDate, endDate)).ReturnsAsync(flights);

            // Act
            var result = await _service.DetectChanges(startDate, endDate, agencyId);

            // Assert
            Assert.That(result.Count(), Is.EqualTo(2));
            Assert.That(result.Any(c => c.FlightId == 1 && c.Status == "New"), Is.True);
            Assert.That(result.Any(c => c.FlightId == 3 && c.Status == "Discontinued"), Is.True);
        }
    }
}