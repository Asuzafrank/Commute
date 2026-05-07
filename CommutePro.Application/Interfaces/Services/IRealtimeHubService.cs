using CommutePro.Application.DTOs.Arrivals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommutePro.Application.Interfaces.Services
{
    public interface IRealtimeHubService
    {
        Task BroadcastArrivalUpdateAsync(string stationId, List<ArrivalDto> arrivals);
        Task BroadcastDelayAlertAsync(string stationId, DelayAlertDto alert);
        Task BroadcastDataStaleAsync(string stationId);
    }
}
