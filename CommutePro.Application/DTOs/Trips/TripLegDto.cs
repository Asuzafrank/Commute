using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommutePro.Application.DTOs.Trips
{
    public class TripLegDto
    {
        public string RouteId { get; set; } = string.Empty;
        public string RouteName { get; set; } = string.Empty;
        public string RouteColor { get; set; } = string.Empty;
        public string RouteTextColor { get; set; } = string.Empty;
        public string FromStopId { get; set; } = string.Empty;
        public string FromStopName { get; set; } = string.Empty;
        public string ToStopId { get; set; } = string.Empty;
        public string ToStopName { get; set; } = string.Empty;
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public int DurationMinutes { get; set; }
    }
}
