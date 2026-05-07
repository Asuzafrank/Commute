using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommutePro.Application.DTOs.GTFS
{
    public class VehiclePositionResponse
    {
        public long Timestamp { get; set; }
        public List<VehiclePosition> Vehicles { get; set; } = new();
    }
}
