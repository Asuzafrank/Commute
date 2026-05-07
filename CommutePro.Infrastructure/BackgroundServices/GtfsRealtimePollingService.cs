using CommutePro.Application.DTOs.Arrivals;
using CommutePro.Application.Interfaces;
using CommutePro.Application.Interfaces.Repositories;
using CommutePro.Application.Interfaces.Services;
using CommutePro.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommutePro.Infrastructure.BackgroundServices
{
    public class GtfsRealtimePollingService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<GtfsRealtimePollingService> _logger;
        private readonly int _pollingIntervalSeconds;

        public GtfsRealtimePollingService(
            IServiceScopeFactory scopeFactory,
            IConfiguration configuration,
            ILogger<GtfsRealtimePollingService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _pollingIntervalSeconds = configuration.GetValue<int>("Gtfs:PollingIntervalSeconds", 15);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("GTFS Realtime Polling Service started. Polling every {Interval} seconds",
                _pollingIntervalSeconds);

            // Poll immediately on startup
            await PollRealtimeFeedsAsync(stoppingToken);

            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(_pollingIntervalSeconds));

            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await PollRealtimeFeedsAsync(stoppingToken);
            }
        }

        private async Task PollRealtimeFeedsAsync(CancellationToken cancellationToken)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var client = scope.ServiceProvider.GetRequiredService<IGtfsRealtimeClient>();
                var cache = scope.ServiceProvider.GetRequiredService<IRealtimeCacheService>();
                var hubService = scope.ServiceProvider.GetRequiredService<IRealtimeHubService>();
                var delayDetection = scope.ServiceProvider.GetRequiredService<IDelayDetectionService>();
                var stationRepo = scope.ServiceProvider.GetRequiredService<IStationRepository>();
                var tripRepo = scope.ServiceProvider.GetRequiredService<ITripRepository>();
                var routeRepo = scope.ServiceProvider.GetRequiredService<IRouteRepository>();

                // Poll all three feeds in parallel
                var tasks = new List<Task>
            {
                PollTripUpdatesAsync(client, cache, cancellationToken),
                PollVehiclePositionsAsync(client, cache, cancellationToken),
                PollServiceAlertsAsync(client, cache, cancellationToken)
            };

                await Task.WhenAll(tasks);

                // After polling, broadcast updates and detect delays
                await BroadcastArrivalUpdatesAsync(stationRepo, tripRepo, routeRepo, hubService, cache, cancellationToken);
                await delayDetection.DetectAndNotifyDelaysAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error polling realtime feeds");
            }
        }

        private async Task PollTripUpdatesAsync(
                 IGtfsRealtimeClient client,
                 IRealtimeCacheService cache,
                 CancellationToken cancellationToken)
        {
            try
            {
                var data = await client.GetTripUpdatesAsync(cancellationToken);

                _logger.LogInformation("PollTripUpdatesAsync: Received {Count} trip updates",
                    data.TripUpdates?.Count ?? 0);

                if (data.TripUpdates?.Any() == true)
                {
                    cache.UpdateTripUpdates(data);
                    _logger.LogInformation("PollTripUpdatesAsync: Updated cache with {Count} trips",
                        data.TripUpdates.Count);
                }
                else
                {
                    _logger.LogWarning("PollTripUpdatesAsync: No trip updates to cache");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to poll TripUpdates");
            }
        }

        private async Task PollVehiclePositionsAsync(
            IGtfsRealtimeClient client,
            IRealtimeCacheService cache,
            CancellationToken cancellationToken)
        {
            try
            {
                var data = await client.GetVehiclePositionsAsync(cancellationToken);
                if (data.Vehicles?.Any() == true)
                {
                    cache.UpdateVehiclePositions(data);
                    _logger.LogDebug("Polled {Count} vehicle positions", data.Vehicles.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to poll VehiclePositions");
            }
        }

        private async Task PollServiceAlertsAsync(
            IGtfsRealtimeClient client,
            IRealtimeCacheService cache,
            CancellationToken cancellationToken)
        {
            try
            {
                var data = await client.GetServiceAlertsAsync(cancellationToken);
                if (data.Alerts?.Any() == true)
                {
                    cache.UpdateServiceAlerts(data);
                    _logger.LogDebug("Polled {Count} service alerts", data.Alerts.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to poll ServiceAlerts");
            }
        }

        private async Task BroadcastArrivalUpdatesAsync(
              IStationRepository stationRepo,
              ITripRepository tripRepo,
              IRouteRepository routeRepo,
              IRealtimeHubService hubService,
              IRealtimeCacheService cache,
              CancellationToken cancellationToken)
        {
            // =========================
            // 1. GET DATA FROM CACHE
            // =========================
            var tripUpdates = cache.GetTripUpdates();
            if (tripUpdates?.TripUpdates == null || !tripUpdates.TripUpdates.Any())
            {
                _logger.LogDebug("No trip updates to broadcast (cache empty)");
                return;
            }

            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            // =========================
            // 2. COLLECT UNIQUE IDs FOR BATCH LOADING
            // =========================
            var tripIds = tripUpdates.TripUpdates
                .Select(t => t.TripId)
                .Where(id => !string.IsNullOrEmpty(id))
                .Distinct()
                .ToList();

            if (!tripIds.Any())
            {
                _logger.LogWarning("No valid trip IDs found in updates");
                return;
            }

            // ✅ Use your existing GetByIdsWithRouteAsync (loads trips WITH routes in one query!)
            var tripsWithRoutes = await tripRepo.GetByIdsWithRouteAsync(tripIds, cancellationToken);

            if (tripsWithRoutes == null || !tripsWithRoutes.Any())
            {
                _logger.LogWarning("No trips found for the given IDs");
                return;
            }

            // Create dictionaries for O(1) lookups
            var tripDict = tripsWithRoutes.ToDictionary(t => t.TripId);

            // Routes are already loaded with trips (via Include), but let's also cache them separately
            var routeDict = tripsWithRoutes
                .Where(t => t.Route != null)
                .Select(t => t.Route!)
                .DistinctBy(r => r.RouteId)
                .ToDictionary(r => r.RouteId);

            // =========================
            // 3. PROCESS ARRIVALS
            // =========================
            var stationArrivals = new Dictionary<string, List<ArrivalDto>>();
            int processedCount = 0;
            int skippedCount = 0;

            foreach (var update in tripUpdates.TripUpdates)
            {
                // Safety limit per poll
                if (processedCount > 5000)
                {
                    _logger.LogWarning("Reached processing limit of 5000 updates");
                    break;
                }

                // Get trip from dictionary (O(1) lookup)
                if (!tripDict.TryGetValue(update.TripId, out var trip))
                {
                    skippedCount++;
                    continue;
                }

                // Get route from dictionary
                routeDict.TryGetValue(trip.RouteId, out var route);

                foreach (var stopUpdate in update.StopTimeUpdates)
                {
                    processedCount++;

                    var stopTime = stopUpdate.ArrivalTime ?? stopUpdate.DepartureTime;

                    // Skip past arrivals (with 60 second grace period)
                    if (!stopTime.HasValue || stopTime.Value < now - 60)
                        continue;

                    // Skip cancelled/skipped
                    if (stopUpdate.ScheduleRelationship == ScheduleRelationship.Canceled ||
                        stopUpdate.ScheduleRelationship == ScheduleRelationship.Skipped)
                        continue;

                    // Get or create station list
                    if (!stationArrivals.TryGetValue(stopUpdate.StopId, out var arrivals))
                    {
                        arrivals = new List<ArrivalDto>();
                        stationArrivals[stopUpdate.StopId] = arrivals;
                    }

                    var delayInfo = FormatDelay(stopUpdate.Delay);

                    var arrival = new ArrivalDto
                    {
                        TripId = update.TripId,
                        RouteId = trip.RouteId,
                        RouteShortName = route?.RouteShortName ?? "Unknown",
                        RouteColor = route?.RouteColor ?? "888888",
                        RouteTextColor = route?.RouteTextColor ?? "FFFFFF",
                        Headsign = trip.TripHeadsign ?? "Unknown",
                        Platform = null,
                        ArrivalTime = stopTime.Value,
                        Delay = stopUpdate.Delay,
                        ScheduleRelationship = stopUpdate.ScheduleRelationship.ToString(),
                        IsCancelled = update.ScheduleRelationship == ScheduleRelationship.Canceled,
                        IsSkipped = stopUpdate.ScheduleRelationship == ScheduleRelationship.Skipped,
                        DisplayTime = FormatArrivalTime(stopTime.Value),
                        Countdown = ComputeCountdown(stopTime.Value),
                        DelayDisplay = delayInfo.display,
                        DelayColor = delayInfo.color
                    };

                    arrivals.Add(arrival);
                }
            }

            _logger.LogInformation(
                "Processed {ProcessedCount} updates, {StationCount} stations, {SkippedCount} skipped",
                processedCount, stationArrivals.Count, skippedCount);

            // =========================
            // 4. BROADCAST TO CLIENTS
            // =========================
            foreach (var (stationId, arrivals) in stationArrivals)
            {
                var sortedArrivals = arrivals
                    .OrderBy(a => a.ArrivalTime)
                    .Take(20)
                    .ToList();

                try
                {
                    await hubService.BroadcastArrivalUpdateAsync(stationId, sortedArrivals);
                    _logger.LogDebug("Broadcasted {Count} arrivals to station {StationId}",
                        sortedArrivals.Count, stationId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to broadcast to station {StationId}", stationId);
                }
            }
        }

        #region Helper Methods

        private string FormatArrivalTime(long unixTime)
        {
            var time = DateTimeOffset.FromUnixTimeSeconds(unixTime).LocalDateTime;
            return time.ToString("HH:mm");
        }

        private string ComputeCountdown(long unixTime)
        {
            var secondsRemaining = unixTime - DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            if (secondsRemaining < 0) return "Departed";
            if (secondsRemaining < 10) return "Due";
            if (secondsRemaining < 60) return $"{secondsRemaining} sec";

            var minutes = secondsRemaining / 60;
            if (minutes < 60) return $"{minutes} min";

            var hours = minutes / 60;
            var remainingMinutes = minutes % 60;
            return $"{hours}h {remainingMinutes}m";
        }

        private (string display, string color) FormatDelay(int? delaySeconds)
        {
            if (!delaySeconds.HasValue) return ("On time", "green");

            if (delaySeconds.Value <= 0) return ("On time", "green");
            if (delaySeconds.Value < 60) return ($"+{delaySeconds.Value} sec", "amber");
            if (delaySeconds.Value < 300) return ($"+{delaySeconds.Value / 60} min", "amber");

            return ($"+{delaySeconds.Value / 60} min", "red");
        }

        #endregion
    }

    public class DelayedTripInfo
    {
        public string TripId { get; set; } = string.Empty;
        public string StopId { get; set; } = string.Empty;
        public string RouteShortName { get; set; } = string.Empty;
        public int DelayMinutes { get; set; }
        public string Headsign { get; set; } = string.Empty;
        public long ArrivalTime { get; set; }
    }
}
