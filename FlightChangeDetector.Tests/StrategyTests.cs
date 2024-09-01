using FlightChangeDetector.Models;
using FlightChangeDetector.Strategies;

namespace FlightChangeDetector.Tests
{
    [TestFixture]
    public class StrategyTests
    {
        [Test]
        public void NewFlightDetectionStrategy_ShouldDetectNewFlights()
        {
            // Arrange
            var strategy = new NewFlightDetectionStrategy();
            var flights = new[]
            {
                new Flight { FlightId = 1, AirlineId = 1, DepartureTime = new DateTime(2023, 1, 1), Route = new Route { OriginCityId = 1, DestinationCityId = 2 } },
                new Flight { FlightId = 2, AirlineId = 1, DepartureTime = new DateTime(2023, 1, 8), Route = new Route { OriginCityId = 1, DestinationCityId = 2 } },
                new Flight { FlightId = 3, AirlineId = 1, DepartureTime = new DateTime(2023, 1, 15), Route = new Route { OriginCityId = 1, DestinationCityId = 2 } },
            };

            // Act
            var result = strategy.DetectChanges(flights);

            // Assert
            Assert.That(result.Count(), Is.EqualTo(1));
            Assert.That(result.First().FlightId, Is.EqualTo(1));
            Assert.That(result.First().Status, Is.EqualTo("New"));
        }

        [Test]
        public void DiscontinuedFlightDetectionStrategy_ShouldDetectDiscontinuedFlights()
        {
            // Arrange
            var strategy = new DiscontinuedFlightDetectionStrategy();
            var flights = new[]
            {
                new Flight { FlightId = 1, AirlineId = 1, DepartureTime = new DateTime(2023, 1, 1), Route = new Route { OriginCityId = 1, DestinationCityId = 2 } },
                new Flight { FlightId = 2, AirlineId = 1, DepartureTime = new DateTime(2023, 1, 8), Route = new Route { OriginCityId = 1, DestinationCityId = 2 } },
                new Flight { FlightId = 3, AirlineId = 1, DepartureTime = new DateTime(2023, 1, 15), Route = new Route { OriginCityId = 1, DestinationCityId = 2 } },
            };

            // Act
            var result = strategy.DetectChanges(flights);

            // Assert
            Assert.That(result.Count(), Is.EqualTo(1));
            Assert.That(result.First().FlightId, Is.EqualTo(3));
            Assert.That(result.First().Status, Is.EqualTo("Discontinued"));
        }
    }
}