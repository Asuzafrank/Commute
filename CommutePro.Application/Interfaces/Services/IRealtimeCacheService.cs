using CommutePro.Application.DTOs.GTFS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommutePro.Application.Interfaces.Services
{
    public interface IRealtimeCacheService
    {
        TripUpdateResponse? GetTripUpdates();
        VehiclePositionResponse? GetVehiclePositions();
        ServiceAlertResponse? GetServiceAlerts();
        void UpdateTripUpdates(TripUpdateResponse data);
        void UpdateVehiclePositions(VehiclePositionResponse data);
        void UpdateServiceAlerts(ServiceAlertResponse data);
        bool IsDataStale { get; }
        long LastUpdateTimestamp { get; }
    }
}
