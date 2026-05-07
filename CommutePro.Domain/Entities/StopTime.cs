using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommutePro.Domain.Entities
{

    public class StopTime
    {
        public string TripId { get; private set; } = string.Empty;
        public int StopSequence { get; private set; }
        public string StopId { get; private set; } = string.Empty;
        public TimeOnly? ArrivalTime { get; private set; }
        public TimeOnly? DepartureTime { get; private set; }
        public byte? PickupType { get; private set; }
        public byte? DropOffType { get; private set; }
        public DateTime CreatedAt { get; private set; }

        public Trip? Trip { get; private set; }
        public Stop? Stop { get; private set; }

        private StopTime() { }

        public static StopTime Create(string tripId, int stopSequence, string stopId,
                              TimeOnly? arrivalTime = null, TimeOnly? departureTime = null,
                              byte? pickupType = null, byte? dropOffType = null)
        {
            if (stopSequence < 1)
                throw new ArgumentException("StopSequence must be >= 1", nameof(stopSequence));

            return new StopTime(tripId, stopSequence, stopId, arrivalTime, departureTime, pickupType, dropOffType);
        }

        public StopTime(string tripId, int stopSequence, string stopId,
                        TimeOnly? arrivalTime = null, TimeOnly? departureTime = null,
                        byte? pickupType = null, byte? dropOffType = null)
        {
            TripId = tripId ?? throw new ArgumentNullException(nameof(tripId));
            StopSequence = stopSequence;
            StopId = stopId ?? throw new ArgumentNullException(nameof(stopId));
            ArrivalTime = arrivalTime;
            DepartureTime = departureTime;
            PickupType = pickupType;
            DropOffType = dropOffType;
            CreatedAt = DateTime.UtcNow;
        }
    }
}
