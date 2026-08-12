using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommutePro.Application.DTOs.Trips
{
    public class TripPlanResponse
    {
        public string FromStation { get; set; } = string.Empty;
        public string ToStation { get; set; } = string.Empty;
        public List<TripLegDto> Legs { get; set; } = new();
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public int TotalDurationMinutes { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
