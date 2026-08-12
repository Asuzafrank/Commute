using CommutePro.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommutePro.Application.DTOs.Trips
{
    public class DirectTripMatch
    {
        public Trip Trip { get; set; } = null!;
        public StopTime FromStopTime { get; set; } = null!;
        public StopTime ToStopTime { get; set; } = null!;
        public Route? Route { get; set; }
    }
}
