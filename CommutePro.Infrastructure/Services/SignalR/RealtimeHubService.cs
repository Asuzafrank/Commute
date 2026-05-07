

using CommutePro.Application.DTOs.Arrivals;
using CommutePro.Application.Interfaces.Services;
using CommutePro.Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommutePro.Infrastructure.Services.SignalR
{
    public class RealtimeHubService : IRealtimeHubService
    {
        private readonly IHubContext<RealtimeHub> _hubContext;
        private readonly ILogger<RealtimeHubService> _logger;
        private readonly IRealtimeCacheService _cache;

        public RealtimeHubService(
            IHubContext<RealtimeHub> hubContext,
            ILogger<RealtimeHubService> logger,
            IRealtimeCacheService cache)
        {
            _hubContext = hubContext;
            _logger = logger;
            _cache = cache;
        }

        /// <summary>
        /// Send updated arrivals to all clients subscribed to this station
        /// </summary>
        public async Task BroadcastArrivalUpdateAsync(string stationId, List<ArrivalDto> arrivals)
        {
            try
            {
                await _hubContext.Clients
                    .Group($"station_{stationId}")
                    .SendAsync("ArrivalsUpdated", new
                    {
                        stationId,
                        arrivals,
                        lastUpdated = DateTime.UtcNow,
                        isStale = _cache.IsDataStale
                    });

                _logger.LogDebug("Broadcasted arrival update to station {StationId}, {Count} arrivals",
                    stationId, arrivals.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to broadcast arrival update for station {StationId}", stationId);
            }
        }

        /// <summary>
        /// Send a delay alert to clients subscribed to this station
        /// </summary>
        public async Task BroadcastDelayAlertAsync(string stationId, DelayAlertDto alert)
        {
            try
            {
                await _hubContext.Clients
                    .Group($"station_{stationId}")
                    .SendAsync("DelayAlert", alert);

                _logger.LogInformation("Broadcasted delay alert to station {StationId}: {DelayMinutes} min delay on {Route}",
                    stationId, alert.DelayMinutes, alert.RouteShortName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to broadcast delay alert for station {StationId}", stationId);
            }
        }

        /// <summary>
        /// Notify clients that data is stale
        /// </summary>
        public async Task BroadcastDataStaleAsync(string stationId)
        {
            try
            {
                await _hubContext.Clients
                    .Group($"station_{stationId}")
                    .SendAsync("DataStale", new { stationId, message = "Data may be outdated" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to broadcast stale data warning for station {StationId}", stationId);
            }
        }
    }
}
