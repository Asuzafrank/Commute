using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommutePro.Domain.Entities
{
    public class CalendarDate
    {
        public string ServiceId { get; internal set; } = string.Empty;
        public DateTime Date { get; private set; }
        public int ExceptionType { get; private set; } // 1 = added, 2 = removed
        public DateTime CreatedAt { get; private set; }

        public Service? Service { get; protected set; }

        private CalendarDate() { }

        public CalendarDate(string serviceId, DateTime date, int exceptionType)
        {
            ServiceId = serviceId;
            Date = date;
            ExceptionType = exceptionType;
            CreatedAt = DateTime.UtcNow;
        }
    }
}
