using CommutePro.Application.Interfaces.BaseRepository;
using CommutePro.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommutePro.Application.Interfaces.Repositories
{
    public interface IStopTimeRepository : IRepository<StopTime, (string tripId, int stopSequence)>
    {
        Task<List<StopTime>> GetByTripAsync(string tripId, CancellationToken cancellationToken = default);
        Task<List<StopTime>> GetByStopAsync(string stopId, CancellationToken cancellationToken = default);
        Task<StopTime?> GetNextStopAsync(string tripId, int currentSequence, CancellationToken cancellationToken = default);
    }
}
