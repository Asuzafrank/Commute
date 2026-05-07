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
    public class FavouriteRepository : GenericRepository<FavouriteStation, Guid>, IFavouriteRepository
    {
        public FavouriteRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<FavouriteStation>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.FavouriteStations
                .Where(f => f.UserId == userId)
                .OrderBy(f => f.SortOrder)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> ExistsAsync(Guid userId, string stopId, CancellationToken cancellationToken = default)
        {
            return await _context.FavouriteStations
                .AnyAsync(f => f.UserId == userId && f.StopId == stopId, cancellationToken);
        }
    }
}
