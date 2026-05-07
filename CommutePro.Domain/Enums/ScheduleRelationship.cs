using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommutePro.Domain.Enums
{
    public enum ScheduleRelationship
    {
        Scheduled = 0,
        Skipped = 1,
        NoData = 2,
        Canceled = 3
    }
}
