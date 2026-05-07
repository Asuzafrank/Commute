using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommutePro.Domain.Entities
{
    
    public class Trip
    {
        public string TripId { get; private set; } = string.Empty;
        public string RouteId { get; private set; } = string.Empty;
        public string ServiceId { get; private set; } = string.Empty;
        public Service? Service { get; private set; }
        public string? TripHeadsign { get; private set; }
        public byte? DirectionId { get; private set; }
        public string? ShapeId { get; private set; }
        public DateTime CreatedAt { get; private set; }

        public Route? Route { get; private set; }
        private readonly List<StopTime> _stopTimes = new();
        public IReadOnlyCollection<StopTime> StopTimes => _stopTimes.AsReadOnly();

        private Trip() { }

        public Trip(string tripId, string routeId, string serviceId,
                    string? tripHeadsign = null, byte? directionId = null, string? shapeId = null)
        {
            TripId = tripId ?? throw new ArgumentNullException(nameof(tripId));
            RouteId = routeId ?? throw new ArgumentNullException(nameof(routeId));
            ServiceId = serviceId ?? throw new ArgumentNullException(nameof(serviceId));
            TripHeadsign = tripHeadsign;
            DirectionId = directionId;
            ShapeId = shapeId;
            CreatedAt = DateTime.UtcNow;
        }

        public void AddStopTime(StopTime stopTime)
        {
            if (_stopTimes.Any(s => s.StopSequence == stopTime.StopSequence))
                throw new InvalidOperationException($"Stop sequence {stopTime.StopSequence} already exists");

            _stopTimes.Add(stopTime);
        }
    }
}
