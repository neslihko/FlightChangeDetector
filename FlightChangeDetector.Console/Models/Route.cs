using CsvHelper.Configuration.Attributes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlightChangeDetector.Models
{
    public class Route
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Name("route_id")]
        public int RouteId { get; set; }

        [Name("origin_city_id")]
        public int OriginCityId { get; set; }

        [Name("destination_city_id")]
        public int DestinationCityId { get; set; }

        [Name("departure_date")]
        public DateTime DepartureDate { get; set; }

        public ICollection<Flight> Flights { get; set; }
    }
}