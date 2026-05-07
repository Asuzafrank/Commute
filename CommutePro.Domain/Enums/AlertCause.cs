using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommutePro.Domain.Enums
{
    public enum AlertCause
    {
        Unknown = 0,
        Accident = 1,
        Construction = 2,
        Demonstration = 3,
        Holiday = 4,
        Maintenance = 5,
        MedicalEmergency = 6,
        PoliceActivity = 7,
        Strike = 8,
        TechnicalProblem = 9,
        Weather = 10,
        OtherCause = 11
    }
}
