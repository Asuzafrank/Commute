using CommutePro.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommutePro.Application.DTOs.GTFS
{
    public class StopTimeUpdate
    {
        public string StopId { get; set; } = string.Empty;
        public int StopSequence { get; set; }
        public long? ArrivalTime { get; set; }
        public long? DepartureTime { get; set; }
        public int? Delay { get; set; }
        public ScheduleRelationship ScheduleRelationship { get; set; }
    }
}
