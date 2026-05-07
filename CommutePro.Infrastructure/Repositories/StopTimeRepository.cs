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
    public class StopTimeRepository : GenericRepository<StopTime, (string tripId, int stopSequence)>, IStopTimeRepository
    {
        public StopTimeRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<StopTime>> GetByTripAsync(string tripId, CancellationToken cancellationToken = default)
        {
            return await _context.StopTimes
                .Where(st => st.TripId == tripId)
                .OrderBy(st => st.StopSequence)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<StopTime>> GetByStopAsync(string stopId, CancellationToken cancellationToken = default)
        {
            return await _context.StopTimes
                .Include(st => st.Trip)
                .ThenInclude(t => t.Route)
                .Where(st => st.StopId == stopId)
                .OrderBy(st => st.DepartureTime)
                .ToListAsync(cancellationToken);
        }

        public async Task<StopTime?> GetNextStopAsync(string tripId, int currentSequence, CancellationToken cancellationToken = default)
        {
            return await _context.StopTimes
                .FirstOrDefaultAsync(st =>
                    st.TripId == tripId &&
                    st.StopSequence > currentSequence,
                    cancellationToken);
        }
    }
}
