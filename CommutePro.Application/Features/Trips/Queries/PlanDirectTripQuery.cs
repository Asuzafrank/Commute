using CommutePro.Application.Common;
using CommutePro.Application.DTOs.Trips;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommutePro.Application.Features.Trips.Queries
{
    public class PlanDirectTripQuery : IRequest<BaseResponse<TripPlanResponse>>
    {
        public string FromStopId { get; set; } = string.Empty;
        public string ToStopId { get; set; } = string.Empty;
        public DateTime DepartureTime { get; set; }
    }
}
