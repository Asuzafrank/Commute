using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommutePro.Application.DTOs.GTFS
{
    public class ServiceAlertResponse
    {
        public List<ServiceAlert> Alerts { get; set; } = new();
    }
}
