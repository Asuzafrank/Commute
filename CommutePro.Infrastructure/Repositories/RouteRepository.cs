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
    public class RouteRepository : GenericRepository<Route, string>, IRouteRepository
    {
        public RouteRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<Route>> GetByAgencyAsync(string agencyId, CancellationToken cancellationToken = default)
        {
            return await _context.Routes
                .Where(r => r.AgencyId == agencyId)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Route>> GetByIdsAsync(List<string> routeIds, CancellationToken cancellationToken = default)
        {
            if (routeIds == null || !routeIds.Any())
                return new List<Route>();

            return await _context.Routes
                .Where(r => routeIds.Contains(r.RouteId))
                .AsNoTracking()  // Read-only for performance
                .ToListAsync(cancellationToken);
        }

        public async Task<Route?> GetWithTripsAsync(string routeId, CancellationToken cancellationToken = default)
        {
            return await _context.Routes
                .Include(r => r.Trips)
                .ThenInclude(t => t.StopTimes)
                .FirstOrDefaultAsync(r => r.RouteId == routeId, cancellationToken);
        }
    }
}
