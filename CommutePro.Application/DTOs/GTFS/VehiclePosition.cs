using CommutePro.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommutePro.Application.DTOs.GTFS
{
    public class VehiclePosition
    {
        public string TripId { get; set; } = string.Empty;
        public string RouteId { get; set; } = string.Empty;
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string? StopId { get; set; }
        public int? CurrentStopSequence { get; set; }
        public VehicleStatus CurrentStatus { get; set; }
        public long Timestamp { get; set; }
        public string? VehicleLabel { get; set; }
        public OccupancyStatus? OccupancyStatus { get; set; }
        public decimal? Bearing { get; set; }
    }
}
