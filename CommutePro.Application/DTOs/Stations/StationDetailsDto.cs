using CommutePro.Application.DTOs.Alerts;
using CommutePro.Application.DTOs.Arrivals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommutePro.Application.DTOs.Stations
{
    public class StationDetailsDto : StationDto
    {
        public List<ArrivalDto> Arrivals { get; set; } = new();
        public List<AlertDto> ActiveAlerts { get; set; } = new();
        public DateTime LastUpdated { get; set; }
        public bool IsDataStale { get; set; }
    }
}
