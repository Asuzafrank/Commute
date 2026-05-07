using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommutePro.Application.DTOs.GTFS
{
    public class InformedEntity
    {
        public string? RouteId { get; set; }
        public string? StopId { get; set; }
        public string? TripId { get; set; }
        public string? AgencyId { get; set; }
    }
}
