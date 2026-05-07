using CommutePro.Application.Interfaces.BaseRepository;
using CommutePro.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommutePro.Application.Interfaces.Repositories
{
    public interface IServiceRepository : IRepository<Service, string>
    {
        Task<Service?> GetWithCalendarDatesAsync(string serviceId, CancellationToken cancellationToken = default);
        Task<bool> IsServiceActiveOnDateAsync(string serviceId, DateTime date, CancellationToken cancellationToken = default);
    }
}
