using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommutePro.Domain.Entities
{
   
    public class Stop
    {
        public string StopId { get; private set; } = string.Empty;
        public string StopName { get; private set; } = string.Empty;
        public decimal? StopLat { get; private set; }
        public decimal? StopLon { get; private set; }
        public string? PlatformCode { get; private set; }
        public byte LocationType { get; private set; } // 0=stop, 1=station
        public string? ParentStation { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        private readonly List<StopTime> _stopTimes = new();
        public IReadOnlyCollection<StopTime> StopTimes => _stopTimes.AsReadOnly();

        private Stop() { }

        public Stop(string stopId, string stopName, byte locationType,
                    string? platformCode = null, decimal? lat = null,
                    decimal? lon = null, string? parentStation = null)
        {
            StopId = stopId ?? throw new ArgumentNullException(nameof(stopId));
            StopName = stopName ?? throw new ArgumentNullException(nameof(stopName));
            LocationType = locationType;
            PlatformCode = platformCode;
            StopLat = lat;
            StopLon = lon;
            ParentStation = parentStation;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
