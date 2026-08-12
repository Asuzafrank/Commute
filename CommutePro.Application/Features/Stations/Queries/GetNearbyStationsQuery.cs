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
    public class GetNearbyStationsQuery : IRequest<BaseResponse<List<NearbyStationDto>>>
    {
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public double RadiusMeters { get; set; } = 1000; // Default 1km
        public int MaxResults { get; set; } = 10;
    }
}
