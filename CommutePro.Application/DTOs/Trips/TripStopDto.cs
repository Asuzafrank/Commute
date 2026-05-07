using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommutePro.Application.DTOs.Trips
{
    public class TripStopDto
    {
        public string StopId { get; set; } = string.Empty;
        public string StopName { get; set; } = string.Empty;
        public string ScheduledTime { get; set; } = string.Empty;
        public string? LiveTime { get; set; }
        public string Status { get; set; } = "future"; // past, current, future
        public int? DelaySeconds { get; set; }
        public bool IsSkipped { get; set; }
    }
}
