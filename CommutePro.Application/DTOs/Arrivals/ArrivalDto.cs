using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommutePro.Application.DTOs.Arrivals
{
    public class ArrivalDto
    {
        public string TripId { get; set; } = string.Empty;
        public string RouteId { get; set; } = string.Empty;
        public string RouteShortName { get; set; } = string.Empty;
        public string RouteColor { get; set; } = string.Empty;
        public string RouteTextColor { get; set; } = string.Empty;
        public string Headsign { get; set; } = string.Empty;
        public string? Platform { get; set; }

        public long ArrivalTime { get; set; } // Unix timestamp
        public int? Delay { get; set; } // Seconds
        public string ScheduleRelationship { get; set; } = "Scheduled";

        // Computed fields
        public string DisplayTime { get; set; } = string.Empty;
        public string Countdown { get; set; } = string.Empty;
        public string DelayDisplay { get; set; } = string.Empty;
        public string DelayColor { get; set; } = string.Empty;
        public bool IsCancelled { get; set; }
        public bool IsSkipped { get; set; }
        public int? PredictedDelay { get; set; }  
        public string? PredictedDelayDisplay { get; set; }
    }
}
