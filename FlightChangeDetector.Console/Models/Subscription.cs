using CsvHelper.Configuration.Attributes;

namespace FlightChangeDetector.Models
{
    public class Subscription
    {
        [Name("agency_id")]
        public int AgencyId { get; set; }

        [Name("origin_city_id")]
        public int OriginCityId { get; set; }

        [Name("destination_city_id")]
        public int DestinationCityId { get; set; }
    }
}