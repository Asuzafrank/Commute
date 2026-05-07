using CommutePro.Application.Common;
using CommutePro.Application.DTOs.Alerts;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommutePro.Application.Features.Alerts.Queries
{
    public class GetActiveAlertsQuery : IRequest<BaseResponse<List<AlertDto>>>
    {
        public string? RouteId { get; set; }
        public string? StopId { get; set; }
    }
}
