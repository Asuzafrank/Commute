using Microsoft.EntityFrameworkCore.Migrations.Operations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommutePro.Domain.Entities
{
    

    public class FavouriteStation
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public string StopId { get; private set; } = string.Empty;
        public int SortOrder { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        public ApplicationUser? User { get; private set; }

        private FavouriteStation() { }

        private FavouriteStation(Guid userId, string stopId, int sortOrder)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            StopId = stopId;
            SortOrder = sortOrder;
            CreatedAt = DateTime.UtcNow;
        }

        public static FavouriteStation Create(Guid userId, string stopId, int sortOrder)
        {
            if (string.IsNullOrWhiteSpace(stopId))
                throw new ArgumentException("StopId cannot be empty", nameof(stopId));

            if (sortOrder < 1)
                throw new ArgumentException("SortOrder must be greater than 0", nameof(sortOrder));

            return new FavouriteStation(userId, stopId, sortOrder);
        }

        internal void UpdateSortOrder(int newSortOrder)
        {
            if(SortOrder == newSortOrder) return;
            SortOrder = newSortOrder;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
