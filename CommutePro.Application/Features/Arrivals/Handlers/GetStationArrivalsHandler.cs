using CommutePro.Application.DTOs.Arrivals;
using CommutePro.Application.Features.Arrivals.Queries;
using CommutePro.Application.Interfaces;
using CommutePro.Application.Interfaces.ML;
using CommutePro.Application.Interfaces.Repositories;
using CommutePro.Application.Interfaces.Services;
using CommutePro.Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CommutePro.Application.Features.Arrivals.Handlers
{
    public class GetStationArrivalsHandler : IRequestHandler<GetStationArrivalsQuery, ArrivalsResponseDto>
    {
        private readonly IStationRepository _stationRepository;
        private readonly ITripRepository _tripRepository;
        private readonly IRouteRepository _routeRepository;
        private readonly IRealtimeCacheService _cache;
        private readonly IDelayPredictionService _delayPredictionService;

        public GetStationArrivalsHandler(
            IStationRepository stationRepository,
            ITripRepository tripRepository,
            IRouteRepository routeRepository,
            IRealtimeCacheService cache,
            IDelayPredictionService delayPredictionService)
        {
            _stationRepository = stationRepository;
            _tripRepository = tripRepository;
            _routeRepository = routeRepository;
            _cache = cache;
            _delayPredictionService = delayPredictionService;
        }

        public async Task<ArrivalsResponseDto> Handle(GetStationArrivalsQuery request, CancellationToken cancellationToken)
        {
            // Get station details
            var stop = await _stationRepository.GetByIdAsync(request.StopId, cancellationToken);

            if (stop == null)
            {
                return new ArrivalsResponseDto
                {
                    StationId = request.StopId,
                    StationName = "Unknown Station",
                    Arrivals = new List<ArrivalDto>(),
                    LastUpdated = DateTime.UtcNow,
                    IsDataStale = true
                };
            }

            // Get realtime data from cache
            var tripUpdates = _cache.GetTripUpdates();

            if (tripUpdates == null)
            {
                return new ArrivalsResponseDto
                {
                    StationId = request.StopId,
                    StationName = stop.StopName,
                    Arrivals = new List<ArrivalDto>(),
                    LastUpdated = DateTime.UtcNow,
                    IsDataStale = true
                };
            }

            var arrivals = new List<ArrivalDto>();
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            foreach (var update in tripUpdates.TripUpdates)
            {
                var stopUpdate = update.StopTimeUpdates
                    .FirstOrDefault(s => s.StopId == request.StopId);

                if (stopUpdate == null) continue;

                // Get the actual stop time (prefer arrival, fallback to departure)
                var stopTime = stopUpdate.ArrivalTime ?? stopUpdate.DepartureTime;

                // Skip if no time or already departed
                if (!stopTime.HasValue || stopTime.Value < now) continue;

                // Skip if cancelled or skipped
                if (stopUpdate.ScheduleRelationship == ScheduleRelationship.Canceled ||
                    stopUpdate.ScheduleRelationship == ScheduleRelationship.Skipped)
                    continue;

                // Try to get trip from database
                var trip = await _tripRepository.GetByIdAsync(update.TripId, cancellationToken);

                ArrivalDto arrival;

                if (trip != null)
                {
                    // Use database data
                    var route = await _routeRepository.GetByIdAsync(trip.RouteId, cancellationToken);

                    // ONLY predict for RAIL routes
                    // MBTA Route Types:
                    // 0 = Light Rail (Green Line)
                    // 1 = Heavy Rail (Red, Orange, Blue Lines)
                    // 2 = Commuter Rail
                    // 3 = Bus (NO prediction)
                    // 4 = Ferry (NO prediction)
                    var isRailRoute = route != null && (route.RouteType == 0 || route.RouteType == 1 || route.RouteType == 2);

                    int predictedDelaySeconds = 0;

                    if (isRailRoute)
                    {
                        var arrivalDateTime = DateTimeOffset.FromUnixTimeSeconds(stopTime.Value).LocalDateTime;
                        var predictedDelay = await _delayPredictionService.PredictDelayAsync(
                            trip.RouteId,
                            arrivalDateTime,
                            0
                        );
                        predictedDelaySeconds = (int)predictedDelay;
                    }

                    arrival = new ArrivalDto
                    {
                        TripId = update.TripId,
                        RouteId = trip.RouteId,
                        RouteShortName = route?.RouteShortName ?? update.RouteId,
                        RouteColor = route?.RouteColor ?? "888888",
                        RouteTextColor = route?.RouteTextColor ?? "FFFFFF",
                        Headsign = trip.TripHeadsign ?? $"Route {update.RouteId}",
                        Platform = stop.PlatformCode,
                        ArrivalTime = stopTime.Value,
                        Delay = stopUpdate.Delay,
                        PredictedDelay = predictedDelaySeconds,
                        ScheduleRelationship = stopUpdate.ScheduleRelationship.ToString(),
                        IsCancelled = update.ScheduleRelationship == ScheduleRelationship.Canceled,
                        IsSkipped = stopUpdate.ScheduleRelationship == ScheduleRelationship.Skipped
                    };
                }
                else
                {
                    // Fall back to realtime data (no prediction for unknown trips)
                    arrival = new ArrivalDto
                    {
                        TripId = update.TripId,
                        RouteId = update.RouteId,
                        RouteShortName = update.RouteId,
                        RouteColor = "888888",
                        RouteTextColor = "FFFFFF",
                        Headsign = $"Route {update.RouteId}",
                        Platform = stop.PlatformCode,
                        ArrivalTime = stopTime.Value,
                        Delay = stopUpdate.Delay,
                        PredictedDelay = 0,
                        ScheduleRelationship = stopUpdate.ScheduleRelationship.ToString(),
                        IsCancelled = update.ScheduleRelationship == ScheduleRelationship.Canceled,
                        IsSkipped = stopUpdate.ScheduleRelationship == ScheduleRelationship.Skipped
                    };
                }

                // Compute display values
                arrival.DisplayTime = FormatArrivalTime(arrival.ArrivalTime);
                arrival.Countdown = ComputeCountdown(arrival.ArrivalTime);
                (arrival.DelayDisplay, arrival.DelayColor) = FormatDelay(arrival.Delay);

                // Add ML prediction display (only if > 0)
                if (arrival.PredictedDelay.HasValue && arrival.PredictedDelay.Value > 0)
                {
                    if (!arrival.Delay.HasValue || arrival.PredictedDelay.Value != arrival.Delay.Value)
                    {
                        arrival.PredictedDelayDisplay = FormatPredictedDelay(arrival.PredictedDelay.Value);
                    }
                }

                arrivals.Add(arrival);
            }

            return new ArrivalsResponseDto
            {
                StationId = request.StopId,
                StationName = stop.StopName,
                Arrivals = arrivals.OrderBy(a => a.ArrivalTime).Take(20).ToList(),
                LastUpdated = DateTimeOffset.FromUnixTimeSeconds(tripUpdates.Timestamp).LocalDateTime,
                IsDataStale = IsDataStale(tripUpdates.Timestamp)
            };
        }

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

            var delayMinutes = delaySeconds.Value / 60;

            if (delaySeconds.Value <= 0) return ("On time", "green");
            if (delaySeconds.Value < 60) return ($"+{delaySeconds.Value} sec", "amber");
            if (delaySeconds.Value < 300) return ($"+{delayMinutes} min", "amber");

            return ($"+{delayMinutes} min", "red");
        }

        private string FormatPredictedDelay(int predictedDelaySeconds)
        {
            var predictedMinutes = predictedDelaySeconds / 60;
            if (predictedDelaySeconds < 60)
                return $"🤖 ML: +{predictedDelaySeconds} sec predicted";
            return $"🤖 ML: +{predictedMinutes} min predicted";
        }

        private bool IsDataStale(long timestamp)
        {
            var age = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - timestamp;
            return age > 60;
        }
    }
}