using CsvHelper;
using CsvHelper.Configuration;
using FlightChangeDetector.Data;
using FlightChangeDetector.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;

namespace FlightChangeDetector.Services
{
    public class DatabaseInitializer
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;
        private const int BatchSize = 20_000; // Adjust this value based on your system's capabilities

        public DatabaseInitializer(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _configuration = configuration;
        }

        public async Task InitializeAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<FlightDbContext>();

            await context.Database.MigrateAsync();

            if (!await context.Routes.AnyAsync())
            {
                await ImportDataAsync(context);
            }
        }

        private async Task ImportDataAsync(FlightDbContext context)
        {
            var routesPath = _configuration["CsvFilePaths:RoutesPath"];
            var flightsPath = _configuration["CsvFilePaths:FlightsPath"];
            var subscriptionsPath = _configuration["CsvFilePaths:SubscriptionsPath"];

            ArgumentException.ThrowIfNullOrWhiteSpace(routesPath);
            ArgumentException.ThrowIfNullOrWhiteSpace(flightsPath);
            ArgumentException.ThrowIfNullOrWhiteSpace(subscriptionsPath);

            await ImportRoutesAsync(context, routesPath);
            await ImportFlightsAsync(context, flightsPath);
            await ImportSubscriptionsAsync(context, subscriptionsPath);
        }

        private async Task ImportRoutesAsync(FlightDbContext context, string relativePath)
        {
            var absolutePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath);

            if (!File.Exists(absolutePath))
            {
                Console.WriteLine($"File not found: {absolutePath}");
                throw new FileNotFoundException($"File not found: {absolutePath}");
            }

            using var reader = new StreamReader(absolutePath);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HeaderValidated = null,
                MissingFieldFound = null
            });

            var routes = csv.GetRecords<Route>();
            int importedCount = 0;

            // Load existing routes into a Dictionary for even faster lookups
            var existingRoutesDict = (await context.Routes.ToListAsync())
                .GroupBy(r => (r.OriginCityId, r.DestinationCityId, r.DepartureDate))
                .ToDictionary(g => g.Key, g => g.First());

            foreach (var batch in BatchEnumerable(routes, BatchSize))
            {
                var newRoutes = new List<Route>();

                foreach (var route in batch)
                {
                    var routeKey = (route.OriginCityId, route.DestinationCityId, route.DepartureDate);

                    if (!existingRoutesDict.TryGetValue(routeKey, out var existingRoute))
                    {
                        newRoutes.Add(new Route
                        {
                            OriginCityId = route.OriginCityId,
                            DestinationCityId = route.DestinationCityId,
                            DepartureDate = route.DepartureDate
                        });
                        existingRoutesDict.Add(routeKey, newRoutes.Last()); // Add to Dictionary for subsequent checks
                    }
                    else
                    {
                        // Update the existing route (if needed)
                        context.Entry(existingRoute).CurrentValues.SetValues(route);
                    }
                }

                // Perform bulk insert operation
                context.Routes.AddRange(newRoutes);
                _ = await context.SaveChangesAsync();

                importedCount += batch.Count();
            }

            Console.WriteLine($"Finished importing {importedCount} routes.");
        }

        private async Task ImportFlightsAsync(FlightDbContext context, string path)
        {
            using var reader = new StreamReader(path);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HeaderValidated = null,
                MissingFieldFound = null
            });

            var flights = csv.GetRecords<Flight>();
            int importedCount = 0;

            if (!flights.Any())
            {
                Console.WriteLine("No flights found in the CSV file.");
                return;
            }

            foreach (var batch in BatchEnumerable(flights, BatchSize))
            {
                var routeIdsInBatch = batch.Select(f => f.RouteId).Distinct().ToList();

                var routes = await context.Routes
                    .Where(r => routeIdsInBatch.Contains(r.RouteId))
                    .ToDictionaryAsync(r => r.RouteId);

                foreach (var flight in batch)
                {
                    if (!routes.TryGetValue(flight.RouteId, out var existingRoute))
                    {
                        continue;
                    }

                    var existingFlight = await context.Flights
                        .FirstOrDefaultAsync(f => f.RouteId == flight.RouteId &&
                            f.DepartureTime == flight.DepartureTime &&
                            f.AirlineId == flight.AirlineId);

                    if (existingFlight == null)
                    {
                        flight.Route = existingRoute;
                        flight.FlightId = 0; // Let the database generate the ID
                        _ = context.Flights.Add(flight);
                    }
                    else
                    {
                        existingFlight.DepartureTime = flight.DepartureTime;
                        existingFlight.ArrivalTime = flight.ArrivalTime;
                        existingFlight.AirlineId = flight.AirlineId;
                        _ = context.Flights.Update(existingFlight);
                    }
                }

                try
                {
                    _ = await context.SaveChangesAsync();
                    importedCount += batch.Count();
                    Console.WriteLine($"Imported {importedCount} flights so far.");
                }
                catch (DbUpdateException ex)
                {
                    Console.WriteLine($"Error importing flights: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    }
                }
            }

            Console.WriteLine($"Finished importing {importedCount} flights.");
        }

        private async Task ImportSubscriptionsAsync(FlightDbContext context, string path)
        {
            using var reader = new StreamReader(path);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HeaderValidated = null,
                MissingFieldFound = null
            });

            var subscriptions = csv.GetRecords<Subscription>();
            int importedCount = 0;

            foreach (var batch in BatchEnumerable(subscriptions, BatchSize))
            {
                var distinctSubscriptions = batch
                    .GroupBy(s => new { s.AgencyId, s.OriginCityId, s.DestinationCityId })
                    .Select(g => g.First())
                    .ToList();

                foreach (var subscription in distinctSubscriptions)
                {
                    var existingSubscription = await context.Subscriptions
                        .FindAsync(subscription.AgencyId, subscription.OriginCityId, subscription.DestinationCityId);
                    if (existingSubscription == null)
                    {
                        _ = context.Subscriptions.Add(subscription);
                    }
                    else
                    {
                        context.Entry(existingSubscription).CurrentValues.SetValues(subscription);
                    }
                }

                _ = await context.SaveChangesAsync();
                importedCount += distinctSubscriptions.Count;
            }

            Console.WriteLine($"Finished importing {importedCount} subscriptions.");
        }

        private IEnumerable<IEnumerable<T>> BatchEnumerable<T>(IEnumerable<T> source, int batchSize)
        {
            using var enumerator = source.GetEnumerator();
            while (enumerator.MoveNext())
            {
                yield return YieldBatchElements(enumerator, batchSize - 1);
            }
        }

        private IEnumerable<T> YieldBatchElements<T>(IEnumerator<T> source, int batchSize)
        {
            yield return source.Current;
            for (int i = 0; i < batchSize && source.MoveNext(); i++)
            {
                yield return source.Current;
            }
        }
    }
}