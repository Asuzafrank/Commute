using CommutePro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommutePro.Application.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<ApplicationUser> Users { get; }
        DbSet<Stop> Stops { get; }
        DbSet<Route> Routes { get; }
        DbSet<Trip> Trips { get; }
        DbSet<StopTime> StopTimes { get; }
        DbSet<FavouriteStation> FavouriteStations { get; }
        DbSet<NotificationPreference> NotificationPreferences { get; }

        DbSet<TEntity> Set<TEntity>() where TEntity : class;

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
