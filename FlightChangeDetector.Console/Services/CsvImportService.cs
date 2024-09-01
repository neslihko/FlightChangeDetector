using CsvHelper;
using CsvHelper.Configuration;
using FlightChangeDetector.Models;
using System.Globalization;

namespace FlightChangeDetector.Services
{
    public interface ICsvOutputService
    {
        void WriteResultsToCsv(IEnumerable<FlightChange> changes, string filePath);
    }

    public class CsvOutputService : ICsvOutputService
    {
        public void WriteResultsToCsv(IEnumerable<FlightChange> changes, string filePath)
        {
            using var writer = new StreamWriter(filePath);
            using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture));

            csv.WriteRecords(changes);
        }
    }
}