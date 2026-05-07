using CommutePro.Application.Interfaces.BaseRepository;
using CommutePro.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommutePro.Application.Interfaces.Repositories
{
    public interface ITripRepository : IRepository<Trip, string>
    {
        Task<List<Trip>> GetByRouteAsync(string routeId, CancellationToken cancellationToken = default);
        Task<List<Trip>> GetByIdsWithRouteAsync(List<string> ids, CancellationToken cancellationToken = default);
        Task<Trip?> GetWithStopTimesAsync(string tripId, CancellationToken cancellationToken = default);
        Task<List<Trip>> GetActiveTripsForDateAsync(DateTime date, CancellationToken cancellationToken = default);
        Task<Trip?> GetTripWithDetailsAsync(string tripId, CancellationToken cancellationToken = default);
    }
}
