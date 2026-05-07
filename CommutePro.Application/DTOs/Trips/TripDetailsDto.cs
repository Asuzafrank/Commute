using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommutePro.Application.DTOs.Trips
{
    public class TripDetailsDto
    {
        public string TripId { get; set; } = string.Empty;
        public string RouteId { get; set; } = string.Empty;
        public string RouteShortName { get; set; } = string.Empty;
        public string RouteColor { get; set; } = string.Empty;
        public string RouteTextColor { get; set; } = string.Empty;
        public string Headsign { get; set; } = string.Empty;
        public string Status { get; set; } = "On Time";
        public int? DelayMinutes { get; set; }
        public List<TripStopDto> Stops { get; set; } = new();
    }
}
