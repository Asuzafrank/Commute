using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommutePro.Application.DTOs.Stations
{
    public class StationDto
    {
        public string StopId { get; set; } = string.Empty;
        public string StopName { get; set; } = string.Empty;
        public string? PlatformCode { get; set; }
        public decimal? StopLat { get; set; }
        public decimal? StopLon { get; set; }
    }
}
