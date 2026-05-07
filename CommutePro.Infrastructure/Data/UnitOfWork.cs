using CommutePro.Application.Interfaces;
using CommutePro.Application.Interfaces.Repositories;
using CommutePro.Domain.Entities;
using CommutePro.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommutePro.Infrastructure.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction? _transaction;
        private bool _disposed;

        // Repositories
        private IStationRepository? _stations;
        private IRouteRepository? _routes;
        private ITripRepository? _trips;
        private IStopTimeRepository? _stopTimes;
        private IFavouriteRepository? _favourites;
        private IUserRepository? _users;
        private IServiceRepository? _services;
        private IAgencyRepository? _agencies;           
        private ICalendarDateRepository? _calendarDates;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public IStationRepository Stations =>
            _stations ??= new StationRepository(_context);

        public IRouteRepository Routes =>
            _routes ??= new RouteRepository(_context);

        public ITripRepository Trips =>
            _trips ??= new TripRepository(_context);

        public IStopTimeRepository StopTimes =>
            _stopTimes ??= new StopTimeRepository(_context);

        public IFavouriteRepository Favourites =>
            _favourites ??= new FavouriteRepository(_context);

        public IUserRepository Users =>
            _users ??= new UserRepository(_context);

        public IServiceRepository Services =>
            _services ??= new ServiceRepository(_context);

        public IAgencyRepository Agencies =>           
            _agencies ??= new AgencyRepository(_context);

        public ICalendarDateRepository CalendarDates => 
            _calendarDates ??= new CalendarDateRepository(_context);

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync(cancellationToken);
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void ClearTracking()
        {
            _context.ChangeTracker.Clear();
        }
        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync(cancellationToken);
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _transaction?.Dispose();
                _context.Dispose();
                _disposed = true;
            }
            GC.SuppressFinalize(this);
        }
    }
}
