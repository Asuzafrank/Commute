using CommutePro.Application.Common;
using CommutePro.Application.DTOs.Stations;
using CommutePro.Application.Features.Stations.Queries;
using CommutePro.Application.Interfaces;
using CommutePro.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommutePro.Application.Features.Stations.Handlers
{
    public class GetStationByIdHandler : IRequestHandler<GetStationByIdQuery, BaseResponse<StationDto>>
    {
        private readonly IStationRepository _stationRepository;

        public GetStationByIdHandler(IStationRepository stationRepository)
        {
            _stationRepository = stationRepository;
        }
        public async Task<BaseResponse<StationDto>> Handle(GetStationByIdQuery request, CancellationToken cancellationToken)
        {
            var stop = await _stationRepository.GetByIdAsync(request.StopId, cancellationToken);

            if (stop == null)
                return BaseResponse<StationDto>.Fail($"Station {request.StopId} not found");

            var dto = new StationDto
            {
                StopId = stop.StopId,
                StopName = stop.StopName,
                PlatformCode = stop.PlatformCode,
                StopLat = stop.StopLat,
                StopLon = stop.StopLon
            };

            return BaseResponse<StationDto>.Ok(dto);
        }
    }
}
