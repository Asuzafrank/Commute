using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommutePro.Application.DTOs.GTFS
{
    public class TripUpdateResponse
    {
        public long Timestamp { get; set; }
        public List<TripUpdate> TripUpdates { get; set; } = new();
    }
}
