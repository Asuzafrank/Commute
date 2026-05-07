using CommutePro.Application.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommutePro.Application.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
        void ClearTracking();
        IStationRepository Stations { get; }
        IRouteRepository Routes { get; }
        ITripRepository Trips { get; }
        IStopTimeRepository StopTimes { get; }
        IFavouriteRepository Favourites { get; }
        IUserRepository Users { get; }
        IAgencyRepository Agencies { get; }
        ICalendarDateRepository CalendarDates { get; }
        IServiceRepository Services { get; }
    }
}
