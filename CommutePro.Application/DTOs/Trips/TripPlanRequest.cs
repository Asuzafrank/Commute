using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommutePro.Application.DTOs.Trips
{
    public class TripPlanRequest
    {
        public string FromStopId { get; set; } = string.Empty;
        public string ToStopId { get; set; } = string.Empty;
        public DateTime DateTime { get; set; }
    }
}
