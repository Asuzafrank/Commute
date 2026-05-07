using CommutePro.Application.Interfaces.BaseRepository;
using CommutePro.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommutePro.Application.Interfaces.Repositories
{
    public interface IRouteRepository : IRepository<Route, string>
    {
        Task<List<Route>> GetByAgencyAsync(string agencyId, CancellationToken cancellationToken = default);
        Task<Route?> GetWithTripsAsync(string routeId, CancellationToken cancellationToken = default);
        Task<List<Route>> GetByIdsAsync(List<string> routeIds,  CancellationToken cancellationToken = default);
    }
}
