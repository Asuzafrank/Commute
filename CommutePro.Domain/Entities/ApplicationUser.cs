using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommutePro.Domain.Entities
{
    

    public class ApplicationUser : IdentityUser<Guid>
    {
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        private readonly List<FavouriteStation> _favouriteStations = new();
        public IReadOnlyCollection<FavouriteStation> FavouriteStations => _favouriteStations.AsReadOnly();

        public NotificationPreference? NotificationPreferences { get; private set; }

        private ApplicationUser() { } // EF Core

        public static ApplicationUser Create(string email, string userName)
        {
            return new ApplicationUser
            {
                Id = Guid.NewGuid(),
                Email = email,
                UserName = userName,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public void AddFavouriteStation(string stopId)
        {
            if (_favouriteStations.Any(f => f.StopId == stopId))
                throw new InvalidOperationException($"Station {stopId} is already in favourites");

            var maxSortOrder = _favouriteStations.Any()
                ? _favouriteStations.Max(f => f.SortOrder)
                : 0;

            var favourite = FavouriteStation.Create(Id, stopId, maxSortOrder + 1);
            _favouriteStations.Add(favourite);
            UpdatedAt = DateTime.UtcNow;
        }

        public void RemoveFavouriteStation(Guid favouriteId)
        {
            var favourite = _favouriteStations.FirstOrDefault(f => f.Id == favouriteId);
            if (favourite == null)
                throw new InvalidOperationException("Favourite station not found");

            _favouriteStations.Remove(favourite);

            // Reorder remaining favourites
            var remaining = _favouriteStations.OrderBy(f => f.SortOrder).ToList();
            for (int i = 0; i < remaining.Count; i++)
            {
                remaining[i].UpdateSortOrder(i + 1);
            }

            UpdatedAt = DateTime.UtcNow;
        }


        public void ReorderFavourites(Dictionary<Guid, int> newOrder)
        {
            foreach (var favourite in _favouriteStations)
            {
                if (newOrder.TryGetValue(favourite.Id, out int newSortOrder))
                {
                    favourite.UpdateSortOrder(newSortOrder);
                }
            }
            UpdatedAt = DateTime.UtcNow;
        }

        public void InitializeNotificationPreferences()
        {
            if (NotificationPreferences == null)
            {
                NotificationPreferences = NotificationPreference.Create(Id);
                UpdatedAt = DateTime.UtcNow;
            }
        }

        public void UpdateNotificationPreferences(int delayThresholdMinutes, bool pushEnabled, bool emailEnabled)
        {
            if (NotificationPreferences == null)
                InitializeNotificationPreferences();

            NotificationPreferences!.Update(delayThresholdMinutes, pushEnabled, emailEnabled);
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
