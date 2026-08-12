using CommutePro.Application.DTOs.Trips;
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
    public class TripRepository : GenericRepository<Trip, string>, ITripRepository
    {
        public TripRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<Trip>> GetByRouteAsync(string routeId, CancellationToken cancellationToken = default)
        {
            return await _context.Trips
                .Where(t => t.RouteId == routeId)
                .ToListAsync(cancellationToken);
        }

        public async Task<Trip?> GetWithStopTimesAsync(string tripId, CancellationToken cancellationToken = default)
        {
            return await _context.Trips
                .Include(t => t.StopTimes)
                .ThenInclude(st => st.Stop)
                .Include(t => t.Route)
                .FirstOrDefaultAsync(t => t.TripId == tripId, cancellationToken);
        }

        public async Task<List<Trip>> GetActiveTripsForDateAsync(DateTime date, CancellationToken cancellationToken = default)
        {
            var dayOfWeek = date.DayOfWeek;

            return await _context.Trips
                .Include(t => t.Service)
                .Where(t => t.Service != null &&
                    date >= t.Service.StartDate &&
                    date <= t.Service.EndDate &&
                    ((dayOfWeek == DayOfWeek.Monday && t.Service.Monday) ||
                     (dayOfWeek == DayOfWeek.Tuesday && t.Service.Tuesday) ||
                     (dayOfWeek == DayOfWeek.Wednesday && t.Service.Wednesday) ||
                     (dayOfWeek == DayOfWeek.Thursday && t.Service.Thursday) ||
                     (dayOfWeek == DayOfWeek.Friday && t.Service.Friday) ||
                     (dayOfWeek == DayOfWeek.Saturday && t.Service.Saturday) ||
                     (dayOfWeek == DayOfWeek.Sunday && t.Service.Sunday)))
                .ToListAsync(cancellationToken);
        }

        public async Task<Trip?> GetTripWithDetailsAsync(string tripId, CancellationToken cancellationToken = default)
        {
            return await _context.Trips
                .Include(t => t.Route)
                .Include(t => t.StopTimes)
                    .ThenInclude(st => st.Stop)
                .FirstOrDefaultAsync(t => t.TripId == tripId, cancellationToken);
        }

        public async Task<List<Trip>> GetByIdsWithRouteAsync(List<string> ids, CancellationToken cancellationToken = default)
        {
            return await _context.Trips
               .Where(t => ids.Contains(t.TripId))
               .Include(t => t.Route)  // Avoids second query for routes
               .AsNoTracking()          // Read-only, better performance
               .ToListAsync(cancellationToken);
        }
        public async Task<List<DirectTripMatch>> FindDirectTripsAsync(string fromStopId, string toStopId, CancellationToken cancellationToken = default)
        {
            var stopTimes = _context.StopTimes.AsQueryable();

            var directTrips = await stopTimes
                .Where(st => st.StopId == fromStopId)
                .SelectMany(st => stopTimes
                    .Where(st2 => st2.StopId == toStopId
                        && st2.TripId == st.TripId
                        && st2.StopSequence > st.StopSequence)
                    .Select(st2 => new DirectTripMatch
                    {
                        Trip = st.Trip!,
                        FromStopTime = st,
                        ToStopTime = st2,
                        Route = st.Trip!.Route
                    }))
                .ToListAsync(cancellationToken);

            return directTrips;
        }
    }
}
