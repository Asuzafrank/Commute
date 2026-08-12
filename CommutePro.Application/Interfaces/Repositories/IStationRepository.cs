using CommutePro.Application.Interfaces.BaseRepository;
using CommutePro.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommutePro.Application.Interfaces.Repositories
{
    public interface IStationRepository : IRepository<Stop, string>
    {
        Task<List<Stop>> SearchByNameAsync(string query, int limit, CancellationToken cancellationToken = default);
        Task<Stop?> GetByPlatformCodeAsync(string platformCode, CancellationToken cancellationToken = default);
        Task<List<Stop>> GetStationsOnlyAsync(CancellationToken cancellationToken = default);
        Task<Stop?> GetWithStopTimesAsync(string stopId, CancellationToken cancellationToken = default);
        Task<List<Stop>> GetAllStationsWithCoordinatesAsync(CancellationToken cancellationToken = default);
    }
}
