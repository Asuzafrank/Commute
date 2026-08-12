using CommutePro.Application.Common;
using CommutePro.Application.DTOs.Trips;
using CommutePro.Application.Features.Trips.Queries;
using CommutePro.Application.Interfaces.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommutePro.Application.Features.Trips.Handlers
{
    public class PlanDirectTripHandler : IRequestHandler<PlanDirectTripQuery, BaseResponse<TripPlanResponse>>
    {
        private readonly ITripRepository _tripRepository;
        private readonly IStationRepository _stationRepository;
        private readonly IRouteRepository _routeRepository;

        public PlanDirectTripHandler(
            ITripRepository tripRepository,
        IStationRepository stationRepository,
        IRouteRepository routeRepository)
        {
            _tripRepository = tripRepository;
            _stationRepository = stationRepository;
            _routeRepository = routeRepository;
        }
        public async Task<BaseResponse<TripPlanResponse>> Handle(PlanDirectTripQuery request, CancellationToken cancellationToken)
        {
            // Get station names
            var fromStation = await _stationRepository.GetByIdAsync(request.FromStopId, cancellationToken);
            var toStation = await _stationRepository.GetByIdAsync(request.ToStopId, cancellationToken);

            if (fromStation == null)
                return BaseResponse<TripPlanResponse>.Fail($"Station {request.FromStopId} not found");

            if (toStation == null)
                return BaseResponse<TripPlanResponse>.Fail($"Station {request.ToStopId} not found");

            // Find direct trips using repository
            var directTrips = await _tripRepository.FindDirectTripsAsync(
                request.FromStopId,
                request.ToStopId,
                cancellationToken);

            if (!directTrips.Any())
            {
                return BaseResponse<TripPlanResponse>.Ok(new TripPlanResponse
                {
                    Success = false,
                    Message = "No direct trips found between these stations",
                    FromStation = fromStation.StopName,
                    ToStation = toStation.StopName
                });
            }

            // Filter and find earliest trip (logic stays in handler - business logic)
            var departureTime = request.DepartureTime;
            var availableTrips = directTrips
                .Where(t => t.FromStopTime.DepartureTime.HasValue)
                .Select(t => new
                {
                    t.Trip,
                    t.Route,
                    t.FromStopTime,
                    t.ToStopTime,
                    DepartureDateTime = GetDateTimeFromTimeOnly(departureTime.Date, t.FromStopTime.DepartureTime.Value),
                    ArrivalDateTime = GetDateTimeFromTimeOnly(departureTime.Date, t.ToStopTime.ArrivalTime ?? t.ToStopTime.DepartureTime!.Value)
                })
                .Where(t => t.DepartureDateTime >= departureTime)
                .OrderBy(t => t.DepartureDateTime)
                .FirstOrDefault();

            if (availableTrips == null)
            {
                return BaseResponse<TripPlanResponse>.Ok(new TripPlanResponse
                {
                    Success = false,
                    Message = "No direct trips found after the specified time",
                    FromStation = fromStation.StopName,
                    ToStation = toStation.StopName
                });
            }
            // Calculate duration
            var durationMinutes = (int)(availableTrips.ArrivalDateTime - availableTrips.DepartureDateTime).TotalMinutes;

            // Build response
            var response = new TripPlanResponse
            {
                Success = true,
                FromStation = fromStation.StopName,
                ToStation = toStation.StopName,
                DepartureTime = availableTrips.DepartureDateTime,
                ArrivalTime = availableTrips.ArrivalDateTime,
                TotalDurationMinutes = durationMinutes,
                Legs = new List<TripLegDto>
        {
            new TripLegDto
            {
                RouteId = availableTrips.Route?.RouteId ?? string.Empty,
                RouteName = availableTrips.Route?.RouteShortName ?? "Unknown",
                RouteColor = availableTrips.Route?.RouteColor ?? "888888",
                RouteTextColor = availableTrips.Route?.RouteTextColor ?? "FFFFFF",
                FromStopId = request.FromStopId,
                FromStopName = fromStation.StopName,
                ToStopId = request.ToStopId,
                ToStopName = toStation.StopName,
                DepartureTime = availableTrips.DepartureDateTime,
                ArrivalTime = availableTrips.ArrivalDateTime,
                DurationMinutes = durationMinutes
            }
        }
            };

            return BaseResponse<TripPlanResponse>.Ok(response);
        }
        private DateTime GetDateTimeFromTimeOnly(DateTime date, TimeOnly time)
        {
            return new DateTime(date.Year, date.Month, date.Day, time.Hour, time.Minute, time.Second);
        }
    }
}
