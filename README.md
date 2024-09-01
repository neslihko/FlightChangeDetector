# Flight Change Detector

Flight Change Detector is a .NET 6 console application that detects changes in flight schedules for airline agencies. It identifies new and discontinued flights based on specified criteria.

## Features

- Detects new and discontinued flights
- Uses Entity Framework Core for data access
- Implements Repository, Strategy, and Behavioral patterns
- Automatically initializes and updates the database on first run
- Imports data from CSV files
- Outputs results to a CSV file

## Prerequisites

- .NET 6 SDK
- SQL Server (or SQL Server Express)

## Getting Started

1. Clone the repository:
   ```
   git clone https://github.com/yourusername/FlightChangeDetector.git
   cd FlightChangeDetector
   ```

2. Install the Entity Framework Core tools globally:
   ```
   dotnet tool install --global dotnet-ef
   ```

3. Update the connection string in `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=FlightChangeDetector;Trusted_Connection=True;"
     },
     "CsvFilePaths": {
       "RoutesPath": "path/to/routes.csv",
       "FlightsPath": "path/to/flights.csv",
       "SubscriptionsPath": "path/to/subscriptions.csv"
     }
   }
   ```

4. Update the CSV file paths in `appsettings.json` to point to your actual CSV files.

5. Build the application:
   ```
   dotnet build
   ```

6. Run the application:
   ```
   dotnet run -- <start_date> <end_date> <agency_id>
   ```
   Example:
   ```
   dotnet run -- 2023-01-01 2023-12-31 1
   ```

   On the first run, the application will automatically create the database and apply any pending migrations.

## Project Structure

- `Data/`: Contains DbContext and database-related classes
- `Models/`: Contains entity classes
- `Repositories/`: Implements the Repository pattern
- `Services/`: Contains business logic services
- `Strategies/`: Implements the Strategy pattern for change detection

## Database Migrations

If you make changes to the database model, create a new migration:

```
dotnet ef migrations add YourMigrationName
```

The migration will be automatically applied the next time you run the application.

## CSV File Format

Ensure your CSV files match the following formats:

1. routes.csv:
   ```
   route_id,origin_city_id,destination_city_id,departure_date
   ```

2. flights.csv:
   ```
   flight_id,route_id,departure_time,arrival_time,airline_id
   ```

3. subscriptions.csv:
   ```
   agency_id,origin_city_id,destination_city_id
   ```

## Running Tests

```
dotnet test
```

## Contributing

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details.

## Troubleshooting

If you encounter issues with Entity Framework Core tools, ensure they're installed globally:

```
dotnet tool install --global dotnet-ef
```

or update them if already installed:

```
dotnet tool update --global dotnet-ef
```