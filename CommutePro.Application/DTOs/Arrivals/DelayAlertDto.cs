using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommutePro.Application.DTOs.Arrivals
{
    public class DelayAlertDto
    {
        public string TripId { get; set; } = string.Empty;
        public string RouteShortName { get; set; } = string.Empty;
        public string RouteColor { get; set; } = string.Empty;
        public string StopId { get; set; } = string.Empty;
        public string StopName { get; set; } = string.Empty;
        public string Headsign { get; set; } = string.Empty;
        public int DelayMinutes { get; set; }
        public int DelaySeconds { get; set; }
        public long ArrivalTime { get; set; }
        public DateTime AlertTime { get; set; }
    }
}
