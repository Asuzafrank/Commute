using CommutePro.Application.Interfaces.BaseRepository;
using CommutePro.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommutePro.Application.Interfaces.Repositories
{
    public interface ICalendarDateRepository : IRepository<CalendarDate, (string serviceId, DateTime date)>
    {
        Task<List<CalendarDate>> GetByServiceIdAsync(string serviceId, CancellationToken cancellationToken = default);
        Task<List<CalendarDate>> GetByDateAsync(DateTime date, CancellationToken cancellationToken = default);
        Task<bool> HasExceptionForDateAsync(string serviceId, DateTime date, int exceptionType, CancellationToken cancellationToken = default);
    }
}
