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
    public class StationRepository : GenericRepository<Stop, string>, IStationRepository
    {
        public StationRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<Stop>> SearchByNameAsync(string query, int limit, CancellationToken cancellationToken = default)
        {
            return await _context.Stops
                .Where(s => s.StopName.Contains(query) || s.StopId.Contains(query))
                .Take(limit)
                .ToListAsync(cancellationToken);
        }

        public async Task<Stop?> GetByPlatformCodeAsync(string platformCode, CancellationToken cancellationToken = default)
        {
            return await _context.Stops
                .FirstOrDefaultAsync(s => s.PlatformCode == platformCode, cancellationToken);
        }

        public async Task<List<Stop>> GetStationsOnlyAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Stops
                .Where(s => s.LocationType == 1)
                .ToListAsync(cancellationToken);
        }

        public async Task<Stop?> GetWithStopTimesAsync(string stopId, CancellationToken cancellationToken = default)
        {
            return await _context.Stops
                .Include(s => s.StopTimes)
                .FirstOrDefaultAsync(s => s.StopId == stopId, cancellationToken);
        }
        public async Task<List<Stop>> GetAllStationsWithCoordinatesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Stops
                .Where(s => s.LocationType == 1 && s.StopLat.HasValue && s.StopLon.HasValue)
                .ToListAsync(cancellationToken);
        }
    }
}
