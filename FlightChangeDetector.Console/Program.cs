using FlightChangeDetector.Data;
using FlightChangeDetector.Repositories;
using FlightChangeDetector.Services;
using FlightChangeDetector.Strategies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Globalization;

namespace FlightChangeDetector
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();


            // Print the connection string
            var connectionString = host.Services.GetRequiredService<IConfiguration>().GetConnectionString("DefaultConnection");
            Console.WriteLine($"Connection String: {connectionString}");

            using (var scope = host.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<FlightDbContext>();
                await dbContext.Database.MigrateAsync(); // Apply any pending migrations
            }

            // Initialize and populate the database
            var databaseInitializer = host.Services.GetRequiredService<DatabaseInitializer>();
            await databaseInitializer.InitializeAsync();
            Console.WriteLine("Database initialization completed successfully.");


            if (args.Length != 3 ||
                         !DateTime.TryParseExact(args[0], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var startDate) ||
                         !DateTime.TryParseExact(args[1], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var endDate) ||
                         !int.TryParse(args[2], out var agencyId))
            {
                Console.WriteLine("Usage: FlightChangeDetector.exe <start_date> <end_date> <agency_id>");
                Console.WriteLine("Date format: yyyy-MM-dd");
                return;
            }

            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var flightChangeService = services.GetRequiredService<IFlightChangeService>();
                    var csvOutputService = services.GetRequiredService<ICsvOutputService>();

                    var changes = await flightChangeService.DetectChanges(startDate, endDate, agencyId);
                    csvOutputService.WriteResultsToCsv(changes, "results.csv");

                    Console.WriteLine($"Detected {changes.Count()} changes. Results written to results.csv");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                }
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
           Host.CreateDefaultBuilder(args)
               .ConfigureServices((hostContext, services) =>
               {
                   _ = services.AddDbContext<FlightDbContext>(options =>
                       options.UseSqlServer(hostContext.Configuration.GetConnectionString("DefaultConnection")));
                   _ = services.AddScoped<IFlightRepository, FlightRepository>();
                   _ = services.AddScoped<IFlightChangeService, FlightChangeService>();
                   _ = services.AddScoped<ICsvOutputService, CsvOutputService>();
                   _ = services.AddScoped<DatabaseInitializer>();

                   // Register strategies
                   _ = services.AddScoped<IFlightDbContext>(provider => provider.GetService<FlightDbContext>());
                   _ = services.AddScoped<IChangeDetectionStrategy, NewFlightDetectionStrategy>();
                   _ = services.AddScoped<IChangeDetectionStrategy, DiscontinuedFlightDetectionStrategy>();
               });
    }
}

