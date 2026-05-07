using CommutePro.Application.Common;
using CommutePro.Application.DTOs.Trips;
using CommutePro.Application.Features.Trips.Queries;
using CommutePro.Application.Interfaces;
using CommutePro.Application.Interfaces.Repositories;
using CommutePro.Application.Interfaces.Services;
using CommutePro.Domain.Entities;
using CommutePro.Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommutePro.Application.Features.Trips.Handlers
{
    public class GetTripDetailsHandler : IRequestHandler<GetTripDetailsQuery, BaseResponse<TripDetailsDto>>
    {
        private readonly ITripRepository _tripRepository;
        private readonly IRouteRepository _routeRepository;
        private readonly IStationRepository _stationRepository;
        private readonly IRealtimeCacheService _cache;

        public GetTripDetailsHandler(
            ITripRepository tripRepository,
            IRouteRepository routeRepository,
            IStationRepository stationRepository,
            IRealtimeCacheService cache)
        {
            _tripRepository = tripRepository;
            _routeRepository = routeRepository;
            _stationRepository = stationRepository;
            _cache = cache;
        }

        public async Task<BaseResponse<TripDetailsDto>> Handle(GetTripDetailsQuery request, CancellationToken cancellationToken)
        {
            // First, try to get from static database
            var trip = await _tripRepository.GetTripWithDetailsAsync(request.TripId, cancellationToken);

            // Get realtime data (always needed for live times)
            var tripUpdates = _cache.GetTripUpdates();
            var realtimeUpdate = tripUpdates?.TripUpdates
                .FirstOrDefault(u => u.TripId == request.TripId);

            // Get vehicle position
            var vehiclePositions = _cache.GetVehiclePositions();
            var vehicle = vehiclePositions?.Vehicles
                .FirstOrDefault(v => v.TripId == request.TripId);

            // If no static data but realtime data exists, build from realtime only
            if (trip == null && realtimeUpdate == null)
            {
                return BaseResponse<TripDetailsDto>.Fail($"Trip {request.TripId} not found");
            }

            // Get route info
            Route? route = null;
            string routeId = trip?.RouteId ?? realtimeUpdate?.RouteId ?? string.Empty;

            if (!string.IsNullOrEmpty(routeId))
            {
                route = await _routeRepository.GetByIdAsync(routeId, cancellationToken);
            }

            // Build stops list
            var stops = new List<TripStopDto>();

            if (trip != null && trip.StopTimes.Any())
            {
                // Use static data for stops
                var currentStopSequence = vehicle?.CurrentStopSequence;
                var currentStopId = vehicle?.StopId;

                foreach (var stopTime in trip.StopTimes.OrderBy(st => st.StopSequence))
                {
                    var realtimeStop = realtimeUpdate?.StopTimeUpdates
                        .FirstOrDefault(s => s.StopId == stopTime.StopId);

                    var stop = await _stationRepository.GetByIdAsync(stopTime.StopId, cancellationToken);

                    // Determine status
                    string status = "future";
                    if (currentStopSequence.HasValue)
                    {
                        if (stopTime.StopSequence < currentStopSequence.Value)
                            status = "past";
                        else if (stopTime.StopSequence == currentStopSequence.Value)
                            status = "current";
                    }
                    else if (currentStopId == stopTime.StopId)
                    {
                        status = "current";
                    }

                    bool isSkipped = realtimeStop?.ScheduleRelationship == ScheduleRelationship.Skipped;

                    string? liveTime = null;
                    int? delaySeconds = realtimeStop?.Delay;

                    if (delaySeconds.HasValue && delaySeconds.Value > 0 && stopTime.DepartureTime.HasValue)
                    {
                        var liveDateTime = DateTime.Today.Add(stopTime.DepartureTime.Value.ToTimeSpan()).AddSeconds(delaySeconds.Value);
                        liveTime = liveDateTime.ToString("HH:mm");
                    }

                    stops.Add(new TripStopDto
                    {
                        StopId = stopTime.StopId,
                        StopName = stop?.StopName ?? stopTime.StopId,
                        ScheduledTime = stopTime.DepartureTime?.ToString("HH:mm") ?? stopTime.ArrivalTime?.ToString("HH:mm") ?? "--:--",
                        LiveTime = liveTime,
                        Status = status,
                        DelaySeconds = delaySeconds,
                        IsSkipped = isSkipped
                    });
                }
            }
            else if (realtimeUpdate != null && realtimeUpdate.StopTimeUpdates.Any())
            {
                // Use only realtime data (no static stops)
                var currentStopSequence = vehicle?.CurrentStopSequence;

                foreach (var stopUpdate in realtimeUpdate.StopTimeUpdates.OrderBy(s => s.StopSequence))
                {
                    var stop = await _stationRepository.GetByIdAsync(stopUpdate.StopId, cancellationToken);

                    string status = "future";
                    if (currentStopSequence.HasValue)
                    {
                        if (stopUpdate.StopSequence < currentStopSequence.Value)
                            status = "past";
                        else if (stopUpdate.StopSequence == currentStopSequence.Value)
                            status = "current";
                    }

                    bool isSkipped = stopUpdate.ScheduleRelationship == ScheduleRelationship.Skipped;

                    string? liveTime = null;
                    if (stopUpdate.ArrivalTime.HasValue)
                    {
                        var time = DateTimeOffset.FromUnixTimeSeconds(stopUpdate.ArrivalTime.Value).LocalDateTime;
                        liveTime = time.ToString("HH:mm");
                    }

                    stops.Add(new TripStopDto
                    {
                        StopId = stopUpdate.StopId,
                        StopName = stop?.StopName ?? stopUpdate.StopId,
                        ScheduledTime = "--:--",
                        LiveTime = liveTime,
                        Status = status,
                        DelaySeconds = stopUpdate.Delay,
                        IsSkipped = isSkipped
                    });
                }
            }

            // Calculate overall trip status
            string tripStatus = "On Time";
            int? totalDelay = realtimeUpdate?.StopTimeUpdates
                .Where(s => s.Delay.HasValue && s.Delay.Value > 0)
                .Select(s => s.Delay.Value)
                .FirstOrDefault();

            if (totalDelay.HasValue && totalDelay.Value > 0)
            {
                tripStatus = "Delayed";
            }

            var result = new TripDetailsDto
            {
                TripId = request.TripId,
                RouteId = routeId,
                RouteShortName = route?.RouteShortName ?? realtimeUpdate?.RouteId ?? "Unknown",
                RouteColor = route?.RouteColor ?? "888888",
                RouteTextColor = route?.RouteTextColor ?? "FFFFFF",
                Headsign = trip?.TripHeadsign ?? $"Trip to {stops.LastOrDefault()?.StopName ?? "Destination"}",
                Status = tripStatus,
                DelayMinutes = totalDelay.HasValue ? totalDelay.Value / 60 : null,
                Stops = stops
            };

            return BaseResponse<TripDetailsDto>.Ok(result);
        }
    }
}
