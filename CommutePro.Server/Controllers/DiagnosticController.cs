using CommutePro.Application.DTOs.GTFS;
using CommutePro.Application.Interfaces;
using CommutePro.Application.Interfaces.Repositories;
using CommutePro.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CommutePro.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DiagnosticController : ControllerBase
    {
        private readonly IRealtimeCacheService _cache;
        private readonly IGtfsRealtimeClient _client;

        public DiagnosticController(IRealtimeCacheService cache, IGtfsRealtimeClient client)
        {
            _cache = cache;
            _client = client;
        }

        [HttpGet("cache")]
        public IActionResult CheckCache()
        {
            var tripUpdates = _cache.GetTripUpdates();
            return Ok(new
            {
                hasCacheData = tripUpdates != null,
                tripCount = tripUpdates?.TripUpdates?.Count ?? 0,
                isStale = _cache.IsDataStale
            });
        }

        [HttpGet("direct")]
        public async Task<IActionResult> CheckDirect()
        {
            var result = await _client.GetTripUpdatesAsync();
            return Ok(new
            {
                count = result.TripUpdates?.Count ?? 0,
                timestamp = result.Timestamp
            });
        }
        [HttpGet("debug/all-stops")]
        public IActionResult DebugAllStops([FromServices] IRealtimeCacheService cache)
        {
            var tripUpdates = cache.GetTripUpdates();
            var allStops = new HashSet<string>();

            foreach (var update in tripUpdates?.TripUpdates ?? new List<TripUpdate>())
            {
                foreach (var stop in update.StopTimeUpdates)
                {
                    allStops.Add(stop.StopId);
                }
            }

            return Ok(new
            {
                totalStops = allStops.Count,
                first20Stops = allStops.Take(20).ToList(),
                contains74614 = allStops.Contains("74614")
            });
        }
        [HttpGet("debug/mbta-raw")]
        public async Task<IActionResult> DebugMbtaRaw([FromServices] HttpClient httpClient)
        {
            var url = "https://cdn.mbta.com/realtime/TripUpdates_enhanced.json";
            var response = await httpClient.GetStringAsync(url);
            var json = System.Text.Json.JsonDocument.Parse(response);

            return Ok(new
            {
                hasEntities = json.RootElement.TryGetProperty("entity", out var entities),
                entityCount = entities.GetArrayLength(),
                firstEntity = entities[0].ToString()
            });
        }
        // Add to any controller temporarily
        [HttpGet("debug/cache-details")]
        public IActionResult DebugCacheDetails([FromServices] IRealtimeCacheService cache)
        {
            var tripUpdates = cache.GetTripUpdates();

            if (tripUpdates == null)
                return Ok(new { message = "Cache is null" });

            return Ok(new
            {
                timestamp = tripUpdates.Timestamp,
                updateCount = tripUpdates.TripUpdates?.Count ?? 0,
                firstUpdate = tripUpdates.TripUpdates?.FirstOrDefault() == null ? null : new
                {
                    tripId = tripUpdates.TripUpdates.First().TripId,
                    routeId = tripUpdates.TripUpdates.First().RouteId,
                    stopCount = tripUpdates.TripUpdates.First().StopTimeUpdates?.Count ?? 0,
                    firstStop = tripUpdates.TripUpdates.First().StopTimeUpdates?.FirstOrDefault()?.StopId
                },
                isStale = cache.IsDataStale
            });
        }
        [HttpGet("debug/client-response")]
        public async Task<IActionResult> DebugClientResponse([FromServices] IGtfsRealtimeClient client)
        {
            var result = await client.GetTripUpdatesAsync();
            return Ok(new
            {
                timestamp = result.Timestamp,
                tripCount = result.TripUpdates?.Count ?? 0,
                firstTrip = result.TripUpdates?.FirstOrDefault() == null ? null : new
                {
                    tripId = result.TripUpdates.First().TripId,
                    routeId = result.TripUpdates.First().RouteId,
                    stopCount = result.TripUpdates.First().StopTimeUpdates?.Count ?? 0
                }
            });
        }
        [HttpGet("debug/stop-time/{stopId}")]
        public IActionResult DebugStopTime(string stopId, [FromServices] IRealtimeCacheService cache)
        {
            var tripUpdates = cache.GetTripUpdates();
            var results = new List<object>();
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            foreach (var update in tripUpdates?.TripUpdates ?? new List<TripUpdate>())
            {
                var stopUpdate = update.StopTimeUpdates.FirstOrDefault(s => s.StopId == stopId);
                if (stopUpdate == null) continue;

                var stopTime = stopUpdate.ArrivalTime ?? stopUpdate.DepartureTime;

                results.Add(new
                {
                    tripId = update.TripId,
                    routeId = update.RouteId,
                    arrivalTime = stopUpdate.ArrivalTime,
                    departureTime = stopUpdate.DepartureTime,
                    stopTime = stopTime,
                    isInFuture = stopTime > now,
                    scheduleRelationship = stopUpdate.ScheduleRelationship.ToString()
                });
            }

            return Ok(new
            {
                now = now,
                nowLocal = DateTimeOffset.FromUnixTimeSeconds(now).LocalDateTime.ToString("HH:mm:ss"),
                stopId = stopId,
                results = results.OrderBy(r => ((dynamic)r).stopTime).Take(10)
            });
        }
        [HttpGet("debug/trip-exists/{tripId}")]
        public async Task<IActionResult> DebugTripExists(string tripId, [FromServices] ITripRepository tripRepo)
        {
            var trip = await tripRepo.GetByIdAsync(tripId);
            return Ok(new
            {
                tripId = tripId,
                exists = trip != null,
                routeId = trip?.RouteId,
                headsign = trip?.TripHeadsign
            });
        }
    }
}
