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
    public class SearchStationsHandler : IRequestHandler<SearchStationsQuery, BaseResponse<List<StationDto>>>
    {
        private readonly IStationRepository _stationRepository;

        public SearchStationsHandler(IStationRepository stationRepository)
        {
            _stationRepository = stationRepository;
        }
        public async Task<BaseResponse<List<StationDto>>> Handle(SearchStationsQuery request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Query) || request.Query.Length < 2)
                return BaseResponse<List<StationDto>>.Ok(new List<StationDto>());

            var stops = await _stationRepository.SearchByNameAsync(request.Query, request.Limit, cancellationToken);

            var dtos = stops.Select(stop => new StationDto
            {
                StopId = stop.StopId,
                StopName = stop.StopName,
                PlatformCode = stop.PlatformCode,
                StopLat = stop.StopLat,
                StopLon = stop.StopLon
            }).ToList();

            return BaseResponse<List<StationDto>>.Ok(dtos);
        }
    }
}
