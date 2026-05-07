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
    public class SearchStationsQuery : IRequest<BaseResponse<List<StationDto>>>
    {
        public string Query { get; set; } = string.Empty;
        public int Limit { get; set; } = 20;
    }
}
