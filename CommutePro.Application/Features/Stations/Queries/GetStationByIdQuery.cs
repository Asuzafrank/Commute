using CommutePro.Application.Common;
using CommutePro.Application.DTOs.Stations;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommutePro.Application.Features.Stations.Queries
{
    public class GetStationByIdQuery : IRequest<BaseResponse<StationDto>>
    {
        public string StopId { get; set; } = string.Empty;
    }
}
