using CommutePro.Application.DTOs.Arrivals;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommutePro.Application.Features.Arrivals.Queries
{
    public class GetStationArrivalsQuery : IRequest<ArrivalsResponseDto>
    {
        public string StopId { get; set; } = string.Empty;
    }
}
