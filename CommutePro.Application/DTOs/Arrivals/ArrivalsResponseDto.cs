using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommutePro.Application.DTOs.Arrivals
{
    public class ArrivalsResponseDto
    {
        public string StationId { get; set; } = string.Empty;
        public string StationName { get; set; } = string.Empty;
        public List<ArrivalDto> Arrivals { get; set; } = new();
        public DateTime LastUpdated { get; set; }
        public bool IsDataStale { get; set; }
    }
}
