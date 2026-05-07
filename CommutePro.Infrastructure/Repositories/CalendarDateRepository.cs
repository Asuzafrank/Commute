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
    public class CalendarDateRepository : GenericRepository<CalendarDate, (string serviceId, DateTime date)>, ICalendarDateRepository
    {
        public CalendarDateRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<CalendarDate>> GetByServiceIdAsync(string serviceId, CancellationToken cancellationToken = default)
        {
            return await _context.CalendarDates
                .Where(cd => cd.ServiceId == serviceId)
                .OrderBy(cd => cd.Date)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<CalendarDate>> GetByDateAsync(DateTime date, CancellationToken cancellationToken = default)
        {
            return await _context.CalendarDates
                .Where(cd => cd.Date == date)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> HasExceptionForDateAsync(string serviceId, DateTime date, int exceptionType, CancellationToken cancellationToken = default)
        {
            return await _context.CalendarDates
                .AnyAsync(cd => cd.ServiceId == serviceId && cd.Date == date && cd.ExceptionType == exceptionType, cancellationToken);
        }
    }
}
