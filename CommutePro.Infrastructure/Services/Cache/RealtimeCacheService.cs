using CommutePro.Application.DTOs.GTFS;
using CommutePro.Application.Interfaces.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommutePro.Infrastructure.Services.Cache
{
    public class RealtimeCacheService : IRealtimeCacheService
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<RealtimeCacheService> _logger;

        private const string TripUpdatesKey = "trip_updates";
        private const string VehiclePositionsKey = "vehicle_positions";
        private const string ServiceAlertsKey = "service_alerts";
        private const string TimestampKey = "realtime_timestamp";

        public RealtimeCacheService(IMemoryCache cache, ILogger<RealtimeCacheService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public TripUpdateResponse? GetTripUpdates()
        {
            return _cache.Get<TripUpdateResponse>(TripUpdatesKey);
        }

        public VehiclePositionResponse? GetVehiclePositions()
        {
            return _cache.Get<VehiclePositionResponse>(VehiclePositionsKey);
        }

        public ServiceAlertResponse? GetServiceAlerts()
        {
            return _cache.Get<ServiceAlertResponse>(ServiceAlertsKey);
        }

        public void UpdateTripUpdates(TripUpdateResponse data)
        {
            _cache.Set(TripUpdatesKey, data, TimeSpan.FromSeconds(60));
            UpdateTimestamp();
            _logger.LogDebug("Updated trip updates, {Count} trips", data.TripUpdates?.Count ?? 0);
        }

        public void UpdateVehiclePositions(VehiclePositionResponse data)
        {
            _cache.Set(VehiclePositionsKey, data, TimeSpan.FromSeconds(60));
            UpdateTimestamp();
            _logger.LogDebug("Updated vehicle positions, {Count} vehicles", data.Vehicles?.Count ?? 0);
        }

        public void UpdateServiceAlerts(ServiceAlertResponse data)
        {
            _cache.Set(ServiceAlertsKey, data, TimeSpan.FromSeconds(60));
            UpdateTimestamp();
            _logger.LogDebug("Updated service alerts, {Count} alerts", data.Alerts?.Count ?? 0);
        }

        public bool IsDataStale
        {
            get
            {
                var lastUpdate = _cache.Get<long?>(TimestampKey);
                if (!lastUpdate.HasValue) return true;

                var age = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - lastUpdate.Value;
                return age > 60; // Stale after 60 seconds
            }
        }

        public long LastUpdateTimestamp => _cache.Get<long?>(TimestampKey) ?? 0;

        private void UpdateTimestamp()
        {
            _cache.Set(TimestampKey, DateTimeOffset.UtcNow.ToUnixTimeSeconds(), TimeSpan.FromSeconds(60));
        }
    }
}
