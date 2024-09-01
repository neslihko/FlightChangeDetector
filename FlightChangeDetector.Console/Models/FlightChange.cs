namespace FlightChangeDetector.Models
{
    public class FlightChange
    {
        public int FlightId { get; }
        public int OriginCityId { get; }
        public int DestinationCityId { get; }
        public DateTime DepartureTime { get; }
        public DateTime ArrivalTime { get; }
        public int AirlineId { get; }
        public string Status { get; }

        public FlightChange(Flight flight, string status)
        {
            FlightId = flight.FlightId;
            OriginCityId = flight.Route.OriginCityId;
            DestinationCityId = flight.Route.DestinationCityId;
            DepartureTime = flight.DepartureTime;
            ArrivalTime = flight.ArrivalTime;
            AirlineId = flight.AirlineId;
            Status = status;
        }
    }
}
