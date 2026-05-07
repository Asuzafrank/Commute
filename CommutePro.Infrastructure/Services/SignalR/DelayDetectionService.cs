using CommutePro.Application.DTOs.Arrivals;
using CommutePro.Application.Interfaces.Repositories;
using CommutePro.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommutePro.Infrastructure.Services.SignalR
{
    public class DelayDetectionService : IDelayDetectionService
    {
        private readonly IRealtimeCacheService _cache;
        private readonly IRealtimeHubService _hubService;
        private readonly IStationRepository _stationRepository;
        private readonly ITripRepository _tripRepository;
        private readonly IRouteRepository _routeRepository;
        private readonly ILogger<DelayDetectionService> _logger;

        // Track already notified delays to avoid spamming
        private readonly HashSet<string> _notifiedDelays = new();
        private readonly TimeSpan _cooldownPeriod = TimeSpan.FromMinutes(5);

        public DelayDetectionService(
            IRealtimeCacheService cache,
            IRealtimeHubService hubService,
            IStationRepository stationRepository,
            ITripRepository tripRepository,
            IRouteRepository routeRepository,
            ILogger<DelayDetectionService> logger)
        {
            _cache = cache;
            _hubService = hubService;
            _stationRepository = stationRepository;
            _tripRepository = tripRepository;
            _routeRepository = routeRepository;
            _logger = logger;
        }

        public async Task DetectAndNotifyDelaysAsync(CancellationToken cancellationToken = default)
        {
            var tripUpdates = _cache.GetTripUpdates();
            if (tripUpdates == null) return;

            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var delayThresholdSeconds = 300; // 5 minutes = significant delay

            foreach (var update in tripUpdates.TripUpdates)
            {
                foreach (var stopUpdate in update.StopTimeUpdates)
                {
                    // Check if this stop has a significant delay
                    if (stopUpdate.Delay.HasValue && stopUpdate.Delay.Value >= delayThresholdSeconds)
                    {
                        var alertKey = $"{update.TripId}_{stopUpdate.StopId}_{stopUpdate.Delay.Value / 60}";

                        // Check if we already notified this delay recently
                        if (_notifiedDelays.Contains(alertKey))
                            continue;

                        // Get stop and trip details
                        var stop = await _stationRepository.GetByIdAsync(stopUpdate.StopId, cancellationToken);
                        if (stop == null) continue;

                        var trip = await _tripRepository.GetByIdAsync(update.TripId, cancellationToken);
                        if (trip == null) continue;

                        var route = await _routeRepository.GetByIdAsync(trip.RouteId, cancellationToken);

                        var alert = new DelayAlertDto
                        {
                            TripId = update.TripId,
                            RouteShortName = route?.RouteShortName ?? "Unknown",
                            RouteColor = route?.RouteColor ?? "888888",
                            StopId = stopUpdate.StopId,
                            StopName = stop.StopName,
                            Headsign = trip.TripHeadsign ?? "Unknown",
                            DelayMinutes = stopUpdate.Delay.Value / 60,
                            DelaySeconds = stopUpdate.Delay.Value,
                            ArrivalTime = stopUpdate.ArrivalTime ?? 0,
                            AlertTime = DateTime.UtcNow
                        };

                        // Broadcast alert to all clients subscribed to this station
                        await _hubService.BroadcastDelayAlertAsync(stopUpdate.StopId, alert);

                        // Track this alert
                        _notifiedDelays.Add(alertKey);

                        _logger.LogInformation("Significant delay detected: {Route} at {Stop} - {DelayMinutes} min",
                            alert.RouteShortName, alert.StopName, alert.DelayMinutes);
                    }
                }
            }

            // Clean up old notifications
            CleanupOldNotifications();
        }

        private void CleanupOldNotifications()
        {
            // Simple cleanup - in production, store with timestamp
            if (_notifiedDelays.Count > 1000)
            {
                _notifiedDelays.Clear();
            }
        }
    }
}
