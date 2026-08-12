using CommutePro.Application.Common;
using CommutePro.Application.DTOs.Stations;
using CommutePro.Application.Features.Stations.Queries;
using CommutePro.Application.Interfaces.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommutePro.Application.Features.Stations.Handlers
{
    public class GetNearbyStationsHandler : IRequestHandler<GetNearbyStationsQuery, BaseResponse<List<NearbyStationDto>>>
    {
        private readonly IStationRepository _stationRepository;

        public GetNearbyStationsHandler(IStationRepository stationRepository)
        {
            _stationRepository = stationRepository;
        }
        public async Task<BaseResponse<List<NearbyStationDto>>> Handle(GetNearbyStationsQuery request, CancellationToken cancellationToken)
        {
            // Get all stations with coordinates from repository
            var stations = await _stationRepository.GetAllStationsWithCoordinatesAsync(cancellationToken);
            var nearbyStations = stations
            .Select(s => new
            {
                Station = s,
                Distance = CalculateDistance(
                    (double)request.Latitude,
                    (double)request.Longitude,
                    (double)s.StopLat!.Value,
                    (double)s.StopLon!.Value
                )
            })
            .Where(x => x.Distance <= request.RadiusMeters)
            .OrderBy(x => x.Distance)
            .Take(request.MaxResults)
            .Select(x => new NearbyStationDto
            {
                StopId = x.Station.StopId,
                StopName = x.Station.StopName,
                PlatformCode = x.Station.PlatformCode,
                Latitude = x.Station.StopLat!.Value,
                Longitude = x.Station.StopLon!.Value,
                DistanceMeters = Math.Round(x.Distance, 1),
                DistanceMiles = Math.Round(x.Distance * 0.000621371, 2)
            })
            .ToList();

            return BaseResponse<List<NearbyStationDto>>.Ok(nearbyStations);
        }

        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371000; // Earth radius in meters
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private double ToRadians(double degrees) => degrees * Math.PI / 180;
    }
}
