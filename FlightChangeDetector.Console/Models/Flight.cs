using CsvHelper.Configuration.Attributes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlightChangeDetector.Models
{
    public class Flight
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Name("flight_id")]
        public int FlightId { get; set; }

        [Name("route_id")]
        public int RouteId { get; set; }

        [Name("departure_time")]
        public DateTime DepartureTime { get; set; }

        [Name("arrival_time")]
        public DateTime ArrivalTime { get; set; }

        [Name("airline_id")]
        public int AirlineId { get; set; }
        [ForeignKey("RouteId")]

        public Route Route { get; set; }
    }
}