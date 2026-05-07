using CommutePro.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommutePro.Application.DTOs.GTFS
{
    public class TripUpdate
    {
        public string TripId { get; set; } = string.Empty;
        public string RouteId { get; set; } = string.Empty;
        public ScheduleRelationship ScheduleRelationship { get; set; }
        public List<StopTimeUpdate> StopTimeUpdates { get; set; } = new();
    }
}
