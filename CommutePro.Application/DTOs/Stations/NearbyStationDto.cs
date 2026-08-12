using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommutePro.Application.DTOs.Stations
{
    public class NearbyStationDto
    {
        public string StopId { get; set; } = string.Empty;
        public string StopName { get; set; } = string.Empty;
        public string? PlatformCode { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public double DistanceMeters { get; set; }
        public double DistanceMiles { get; set; }
    }
}
