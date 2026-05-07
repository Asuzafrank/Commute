using CommutePro.Application.Interfaces;
using CommutePro.Domain.Entities;
using CommutePro.Infrastructure.Data;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommutePro.Infrastructure.Seeder
{
    public class GtfsStaticImporter
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<GtfsStaticImporter> _logger;

        public GtfsStaticImporter(IUnitOfWork unitOfWork, ILogger<GtfsStaticImporter> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ImportResult> ImportFromFilesAsync(string folderPath, CancellationToken cancellationToken = default)
        {
            var result = new ImportResult { ImportedAt = DateTime.UtcNow };

            try
            {
                _logger.LogInformation("Starting GTFS static import from {FolderPath}", folderPath);

                // Parse all files
                var agencies = await ParseAgenciesAsync(Path.Combine(folderPath, "agency.txt"), cancellationToken);
                var stops = await ParseStopsAsync(Path.Combine(folderPath, "stops.txt"), cancellationToken);
                var routes = await ParseRoutesAsync(Path.Combine(folderPath, "routes.txt"), cancellationToken);
                var services = await ParseServicesAsync(Path.Combine(folderPath, "calendar.txt"), cancellationToken);
                var calendarDates = await ParseCalendarDatesAsync(Path.Combine(folderPath, "calendar_dates.txt"), cancellationToken);
                var trips = await ParseTripsAsync(Path.Combine(folderPath, "trips.txt"), cancellationToken);
                var stopTimes = await ParseStopTimesAsync(Path.Combine(folderPath, "stop_times.txt"), cancellationToken);

                _logger.LogInformation(
                    "Parsed: {AgencyCount} agencies, {StopCount} stops, {RouteCount} routes, {ServiceCount} services, {CalendarDateCount} calendar dates, {TripCount} trips, {StopTimeCount} stop times",
                    agencies.Count, stops.Count, routes.Count, services.Count, calendarDates.Count, trips.Count, stopTimes.Count);

                // Begin transaction
                await _unitOfWork.BeginTransactionAsync(cancellationToken);

                try
                {
                    // Clear existing data in correct order
                    await ClearExistingDataAsync(cancellationToken);

                    // Import agencies
                    if (agencies.Any())
                    {
                        await _unitOfWork.Agencies.AddRangeAsync(agencies, cancellationToken);
                        await _unitOfWork.SaveChangesAsync(cancellationToken);
                        _logger.LogInformation("✅ Added {Count} agencies", agencies.Count);
                    }

                    // Import routes
                    if (routes.Any())
                    {
                        await _unitOfWork.Routes.AddRangeAsync(routes, cancellationToken);
                        await _unitOfWork.SaveChangesAsync(cancellationToken);
                        _logger.LogInformation("✅ Added {Count} routes", routes.Count);
                    }

                    // Import services
                    if (services.Any())
                    {
                        await _unitOfWork.Services.AddRangeAsync(services, cancellationToken);
                        await _unitOfWork.SaveChangesAsync(cancellationToken);
                        _logger.LogInformation("✅ Added {Count} services", services.Count);
                    }

                    // Import calendar dates
                    if (calendarDates.Any())
                    {
                        await _unitOfWork.CalendarDates.AddRangeAsync(calendarDates, cancellationToken);
                        await _unitOfWork.SaveChangesAsync(cancellationToken);
                        _logger.LogInformation("✅ Added {Count} calendar dates", calendarDates.Count);
                    }

                    // Import trips
                    if (trips.Any())
                    {
                        await _unitOfWork.Trips.AddRangeAsync(trips, cancellationToken);
                        await _unitOfWork.SaveChangesAsync(cancellationToken);
                        _logger.LogInformation("✅ Added {Count} trips", trips.Count);
                    }

                    // Import stops
                    if (stops.Any())
                    {
                        await _unitOfWork.Stations.AddRangeAsync(stops, cancellationToken);
                        await _unitOfWork.SaveChangesAsync(cancellationToken);
                        _logger.LogInformation("✅ Added {Count} stops", stops.Count);
                    }

                    // Import stop times with optimized batching
                    if (stopTimes.Any())
                    {
                        await ImportStopTimesOptimizedAsync(stopTimes, cancellationToken);
                    }

                    await _unitOfWork.CommitTransactionAsync(cancellationToken);

                    result.Success = true;
                    result.Message = "Import completed successfully";
                    result.AgenciesImported = agencies.Count;
                    result.StopsImported = stops.Count;
                    result.RoutesImported = routes.Count;
                    result.ServicesImported = services.Count;
                    result.CalendarDatesImported = calendarDates.Count;
                    result.TripsImported = trips.Count;
                    result.StopTimesImported = stopTimes.Count;

                    _logger.LogInformation("✅ GTFS static import completed successfully!");
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    _logger.LogError(ex, "Error during transaction, rolling back");
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to import GTFS static data");
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }

        private async Task ImportStopTimesOptimizedAsync(List<StopTime> stopTimes, CancellationToken cancellationToken)
        {
            const int batchSize = 10000;
            int totalStopTimes = stopTimes.Count;
            int imported = 0;
            int batchNumber = 0;

            _logger.LogInformation("🚀 Starting optimized import of {Total:N0} stop times with batch size {BatchSize:N0}", totalStopTimes, batchSize);

            var stopTimeEntities = new List<StopTime>(batchSize);

            for (int i = 0; i < totalStopTimes; i++)
            {
                stopTimeEntities.Add(stopTimes[i]);

                // batch is full or it's the last item
                if (stopTimeEntities.Count >= batchSize || i == totalStopTimes - 1)
                {
                    batchNumber++;
                    int currentBatchSize = stopTimeEntities.Count;

                    _logger.LogInformation("📦 Processing batch {BatchNumber} - {BatchSize:N0} records", batchNumber, currentBatchSize);

                    // Adding the entire batch at once
                    await _unitOfWork.StopTimes.AddRangeAsync(stopTimeEntities, cancellationToken);

                    // Save to database
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    imported += currentBatchSize;
                    int percentage = (imported * 100) / totalStopTimes;

                    _logger.LogInformation("✅ Batch {BatchNumber} complete: {Imported:N0}/{Total:N0} stop times ({Percentage}%) - Memory cleared",
                        batchNumber, imported, totalStopTimes, percentage);

                    // Clear the batch list for next iteration
                    stopTimeEntities.Clear();

                    // Clear EF Core's change tracker to free memory
                    _unitOfWork.ClearTracking();

                    // Small delay to prevent overwhelming the database
                    if (batchNumber % 10 == 0)
                    {
                        await Task.Delay(100, cancellationToken);
                        _logger.LogInformation("🔄 Taking a brief pause after {BatchNumber} batches to allow database to catch up", batchNumber);
                    }
                }
            }

            _logger.LogInformation("🎉 Successfully imported all {Total:N0} stop times in {BatchCount} batches!", totalStopTimes, batchNumber);
        }

        private async Task<List<Agency>> ParseAgenciesAsync(string filePath, CancellationToken cancellationToken)
        {
            if (!File.Exists(filePath))
            {
                _logger.LogWarning("agency.txt not found, skipping");
                return new List<Agency>();
            }

            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HeaderValidated = null,
                MissingFieldFound = null
            });

            var agencies = new List<Agency>();

            await csv.ReadAsync();
            csv.ReadHeader();

            while (await csv.ReadAsync())
            {
                var agencyId = csv.GetField("agency_id") ?? "MBTA";
                var agencyName = csv.GetField("agency_name") ?? "Transit Agency";
                var agencyUrl = csv.GetField("agency_url");
                var agencyTimezone = csv.GetField("agency_timezone") ?? "America/New_York";
                var agencyLang = csv.GetField("agency_lang");
                var agencyPhone = csv.GetField("agency_phone");

                agencies.Add(new Agency(agencyId, agencyName, agencyTimezone, agencyUrl, agencyLang, agencyPhone));
            }

            return agencies;
        }

        private async Task<List<Stop>> ParseStopsAsync(string filePath, CancellationToken cancellationToken)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"stops.txt not found: {filePath}");

            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HeaderValidated = null,
                MissingFieldFound = null
            });

            var stops = new List<Stop>();

            await csv.ReadAsync();
            csv.ReadHeader();

            while (await csv.ReadAsync())
            {
                var stopId = csv.GetField("stop_id");
                if (string.IsNullOrEmpty(stopId)) continue;

                var stopName = csv.GetField("stop_name") ?? stopId;
                var locationType = csv.GetField<byte?>("location_type") ?? 0;
                var platformCode = csv.GetField("platform_code");
                var stopLat = csv.GetField<decimal?>("stop_lat");
                var stopLon = csv.GetField<decimal?>("stop_lon");
                var parentStation = csv.GetField("parent_station");

                // Only include stations (location_type = 0 or 1) or stops with platform code
                if (locationType == 0 || locationType == 1 || !string.IsNullOrEmpty(platformCode))
                {
                    stops.Add(new Stop(
                        stopId: stopId,
                        stopName: stopName,
                        locationType: locationType,
                        platformCode: platformCode,
                        lat: stopLat,
                        lon: stopLon,
                        parentStation: parentStation
                    ));
                }
            }

            return stops;
        }

        private async Task<List<Route>> ParseRoutesAsync(string filePath, CancellationToken cancellationToken)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"routes.txt not found: {filePath}");

            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HeaderValidated = null,
                MissingFieldFound = null
            });

            var routes = new List<Route>();

            await csv.ReadAsync();
            csv.ReadHeader();

            while (await csv.ReadAsync())
            {
                var routeId = csv.GetField("route_id");
                if (string.IsNullOrEmpty(routeId)) continue;

                var routeShortName = csv.GetField("route_short_name") ?? routeId;
                var routeLongName = csv.GetField("route_long_name");
                var routeType = csv.GetField<byte>("route_type");
                var agencyId = csv.GetField("agency_id") ?? "MBTA";
                var routeColor = csv.GetField("route_color");
                var routeTextColor = csv.GetField("route_text_color");

                routes.Add(new Route(
                    routeId: routeId,
                    routeShortName: routeShortName,
                    routeType: routeType,
                    agencyId: agencyId,
                    routeLongName: routeLongName,
                    routeColor: routeColor,
                    routeTextColor: routeTextColor
                ));
            }

            return routes;
        }

        private async Task<List<Service>> ParseServicesAsync(string filePath, CancellationToken cancellationToken)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"calendar.txt not found: {filePath}");

            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HeaderValidated = null,
                MissingFieldFound = null
            });

            var services = new List<Service>();

            await csv.ReadAsync();
            csv.ReadHeader();

            while (await csv.ReadAsync())
            {
                var serviceId = csv.GetField("service_id");
                if (string.IsNullOrEmpty(serviceId)) continue;

                var monday = csv.GetField("monday") == "1";
                var tuesday = csv.GetField("tuesday") == "1";
                var wednesday = csv.GetField("wednesday") == "1";
                var thursday = csv.GetField("thursday") == "1";
                var friday = csv.GetField("friday") == "1";
                var saturday = csv.GetField("saturday") == "1";
                var sunday = csv.GetField("sunday") == "1";

                var startDateStr = csv.GetField("start_date");
                var endDateStr = csv.GetField("end_date");

                if (string.IsNullOrEmpty(startDateStr) || string.IsNullOrEmpty(endDateStr))
                    continue;

                var startDate = DateTime.ParseExact(startDateStr, "yyyyMMdd", CultureInfo.InvariantCulture);
                var endDate = DateTime.ParseExact(endDateStr, "yyyyMMdd", CultureInfo.InvariantCulture);

                services.Add(new Service(
                    serviceId: serviceId,
                    monday: monday,
                    tuesday: tuesday,
                    wednesday: wednesday,
                    thursday: thursday,
                    friday: friday,
                    saturday: saturday,
                    sunday: sunday,
                    startDate: startDate,
                    endDate: endDate
                ));
            }

            return services;
        }

        private async Task<List<CalendarDate>> ParseCalendarDatesAsync(string filePath, CancellationToken cancellationToken)
        {
            if (!File.Exists(filePath))
            {
                _logger.LogWarning("calendar_dates.txt not found, skipping");
                return new List<CalendarDate>();
            }

            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HeaderValidated = null,
                MissingFieldFound = null
            });

            var calendarDates = new List<CalendarDate>();

            await csv.ReadAsync();
            csv.ReadHeader();

            while (await csv.ReadAsync())
            {
                var serviceId = csv.GetField("service_id");
                var dateStr = csv.GetField("date");
                var exceptionTypeStr = csv.GetField("exception_type");

                if (string.IsNullOrEmpty(serviceId) || string.IsNullOrEmpty(dateStr) || string.IsNullOrEmpty(exceptionTypeStr))
                    continue;

                var date = DateTime.ParseExact(dateStr, "yyyyMMdd", CultureInfo.InvariantCulture);
                var exceptionType = int.Parse(exceptionTypeStr);

                calendarDates.Add(new CalendarDate(serviceId, date, exceptionType));
            }

            return calendarDates;
        }

        private async Task<List<Trip>> ParseTripsAsync(string filePath, CancellationToken cancellationToken)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"trips.txt not found: {filePath}");

            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HeaderValidated = null,
                MissingFieldFound = null
            });

            var trips = new List<Trip>();

            await csv.ReadAsync();
            csv.ReadHeader();

            while (await csv.ReadAsync())
            {
                var tripId = csv.GetField("trip_id");
                if (string.IsNullOrEmpty(tripId)) continue;

                var routeId = csv.GetField("route_id");
                var serviceId = csv.GetField("service_id");
                var tripHeadsign = csv.GetField("trip_headsign");
                var directionId = csv.GetField<byte?>("direction_id");
                var shapeId = csv.GetField("shape_id");

                if (string.IsNullOrEmpty(routeId) || string.IsNullOrEmpty(serviceId))
                    continue;

                trips.Add(new Trip(
                    tripId: tripId,
                    routeId: routeId,
                    serviceId: serviceId,
                    tripHeadsign: tripHeadsign,
                    directionId: directionId,
                    shapeId: shapeId
                ));
            }

            return trips;
        }

        private async Task<List<StopTime>> ParseStopTimesAsync(string filePath, CancellationToken cancellationToken)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"stop_times.txt not found: {filePath}");

            _logger.LogInformation("📖 Parsing stop_times.txt (this may take a few minutes for 1.4M records)...");

            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HeaderValidated = null,
                MissingFieldFound = null,
                BufferSize = 0x10000 // 64KB buffer for better performance
            });

            var stopTimes = new List<StopTime>();
            int processedCount = 0;
            int lastLoggedCount = 0;

            await csv.ReadAsync();
            csv.ReadHeader();

            while (await csv.ReadAsync())
            {
                var tripId = csv.GetField("trip_id");
                var stopSequence = csv.GetField<int?>("stop_sequence");
                var stopId = csv.GetField("stop_id");

                if (string.IsNullOrEmpty(tripId) || !stopSequence.HasValue || string.IsNullOrEmpty(stopId))
                    continue;

                var arrivalTime = ParseGtfsTime(csv.GetField("arrival_time"));
                var departureTime = ParseGtfsTime(csv.GetField("departure_time"));
                var pickupType = csv.GetField<byte?>("pickup_type");
                var dropOffType = csv.GetField<byte?>("drop_off_type");

                stopTimes.Add(new StopTime(
                    tripId: tripId,
                    stopSequence: stopSequence.Value,
                    stopId: stopId,
                    arrivalTime: arrivalTime,
                    departureTime: departureTime,
                    pickupType: pickupType,
                    dropOffType: dropOffType
                ));

                processedCount++;

                // Log progress every 100,000 records during parsing
                if (processedCount - lastLoggedCount >= 100000)
                {
                    lastLoggedCount = processedCount;
                    _logger.LogInformation("📊 Parsed {Count:N0} stop times so far...", processedCount);
                }
            }

            _logger.LogInformation("✅ Finished parsing {Count:N0} stop times", stopTimes.Count);
            return stopTimes;
        }

        private TimeOnly? ParseGtfsTime(string? timeString)
        {
            if (string.IsNullOrWhiteSpace(timeString))
                return null;

            var parts = timeString.Split(':');
            if (parts.Length != 3)
                return null;

            if (!int.TryParse(parts[0], out var hours))
                return null;
            if (!int.TryParse(parts[1], out var minutes))
                return null;
            if (!int.TryParse(parts[2], out var seconds))
                return null;

            // Handle times beyond 23:59:59 (next day)
            if (hours >= 24)
            {
                hours -= 24;
            }

            if (hours < 0 || hours > 23 || minutes < 0 || minutes > 59 || seconds < 0 || seconds > 59)
                return null;

            return new TimeOnly(hours, minutes, seconds);
        }

        private async Task ClearExistingDataAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("🧹 Clearing existing GTFS data...");

            // Clear in correct order due to foreign keys
            await _unitOfWork.StopTimes.ClearAllAsync(cancellationToken);
            await _unitOfWork.Trips.ClearAllAsync(cancellationToken);
            await _unitOfWork.CalendarDates.ClearAllAsync(cancellationToken);
            await _unitOfWork.Services.ClearAllAsync(cancellationToken);
            await _unitOfWork.Routes.ClearAllAsync(cancellationToken);
            await _unitOfWork.Stations.ClearAllAsync(cancellationToken);
            await _unitOfWork.Agencies.ClearAllAsync(cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("✅ Cleared existing GTFS data");
        }
    }

    public class ImportResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int AgenciesImported { get; set; }
        public int StopsImported { get; set; }
        public int RoutesImported { get; set; }
        public int ServicesImported { get; set; }
        public int CalendarDatesImported { get; set; }
        public int TripsImported { get; set; }
        public int StopTimesImported { get; set; }
        public DateTime ImportedAt { get; set; }
    }
}