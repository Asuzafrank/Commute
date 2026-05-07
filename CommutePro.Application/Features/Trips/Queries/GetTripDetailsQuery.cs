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
    public class GetTripDetailsQuery : IRequest<BaseResponse<TripDetailsDto>>
    {
        public string TripId { get; set; } = string.Empty;
    }
}
