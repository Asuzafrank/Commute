using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommutePro.Domain.Entities
{
    public class Service
    {
        public string ServiceId { get; private set; } = string.Empty;
        public bool Monday { get; private set; }
        public bool Tuesday { get; private set; }
        public bool Wednesday { get; private set; }
        public bool Thursday { get; private set; }
        public bool Friday { get; private set; }
        public bool Saturday { get; private set; }
        public bool Sunday { get; private set; }
        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }
        public DateTime CreatedAt { get; private set; }
        private readonly List<CalendarDate> _calendarDates = new();
        public IReadOnlyCollection<CalendarDate> CalendarDates => _calendarDates.AsReadOnly();

        private Service() { }

        public Service(string serviceId, bool monday, bool tuesday, bool wednesday,
                       bool thursday, bool friday, bool saturday, bool sunday,
                       DateTime startDate, DateTime endDate)
        {
            ServiceId = serviceId;
            Monday = monday;
            Tuesday = tuesday;
            Wednesday = wednesday;
            Thursday = thursday;
            Friday = friday;
            Saturday = saturday;
            Sunday = sunday;
            StartDate = startDate;
            EndDate = endDate;
            CreatedAt = DateTime.UtcNow;
        }

        public bool IsActiveOnDate(DateTime date)
        {
            if (date < StartDate || date > EndDate)
                return false;

            return date.DayOfWeek switch
            {
                DayOfWeek.Monday => Monday,
                DayOfWeek.Tuesday => Tuesday,
                DayOfWeek.Wednesday => Wednesday,
                DayOfWeek.Thursday => Thursday,
                DayOfWeek.Friday => Friday,
                DayOfWeek.Saturday => Saturday,
                DayOfWeek.Sunday => Sunday,
                _ => false
            };
        }
    }
}
