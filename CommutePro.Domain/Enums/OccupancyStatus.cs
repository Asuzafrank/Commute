using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommutePro.Domain.Enums
{
    public enum OccupancyStatus
    {
        Empty = 0,
        ManySeatsAvailable = 1,
        FewSeatsAvailable = 2,
        StandingRoomOnly = 3,
        CrushedStandingRoomOnly = 4,
        Full = 5,
        NotAcceptingPassengers = 6
    }
}
