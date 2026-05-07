using CommutePro.Application.Interfaces.Repositories;
using CommutePro.Domain.Entities;
using CommutePro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommutePro.Infrastructure.Repositories
{
    public class ServiceRepository : GenericRepository<Service, string>, IServiceRepository
    {
        public ServiceRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Service?> GetWithCalendarDatesAsync(string serviceId, CancellationToken cancellationToken = default)
        {
            return await _context.Services
                .Include(s => s.CalendarDates)
                .FirstOrDefaultAsync(s => s.ServiceId == serviceId, cancellationToken);
        }

        public async Task<bool> IsServiceActiveOnDateAsync(string serviceId, DateTime date, CancellationToken cancellationToken = default)
        {
            var dayOfWeek = date.DayOfWeek;

            return await _context.Services
                .Where(s => s.ServiceId == serviceId &&
                    date >= s.StartDate &&
                    date <= s.EndDate &&
                    ((dayOfWeek == DayOfWeek.Monday && s.Monday) ||
                     (dayOfWeek == DayOfWeek.Tuesday && s.Tuesday) ||
                     (dayOfWeek == DayOfWeek.Wednesday && s.Wednesday) ||
                     (dayOfWeek == DayOfWeek.Thursday && s.Thursday) ||
                     (dayOfWeek == DayOfWeek.Friday && s.Friday) ||
                     (dayOfWeek == DayOfWeek.Saturday && s.Saturday) ||
                     (dayOfWeek == DayOfWeek.Sunday && s.Sunday)))
                .AnyAsync(cancellationToken);
        }
    }
}
