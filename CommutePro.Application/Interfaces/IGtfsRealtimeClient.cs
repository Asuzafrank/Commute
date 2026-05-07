using CommutePro.Application.DTOs.GTFS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommutePro.Application.Interfaces
{
    public interface IGtfsRealtimeClient
    {
        Task<TripUpdateResponse?> GetTripUpdatesAsync(CancellationToken cancellationToken = default);
        Task<VehiclePositionResponse?> GetVehiclePositionsAsync(CancellationToken cancellationToken = default);
        Task<ServiceAlertResponse?> GetServiceAlertsAsync(CancellationToken cancellationToken = default);
    }
}
