using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommutePro.Domain.Enums
{
    public enum AlertEffect
    {
        UnknownEffect = 0,
        NoService = 1,
        ReducedService = 2,
        SignificantDelays = 3,
        Detour = 4,
        AdditionalService = 5,
        ModifiedService = 6,
        OtherEffect = 7,
        StopMoved = 8,
        NoEffect = 9,
        AccessibilityIssue = 10
    }
}
