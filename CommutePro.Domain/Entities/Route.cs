using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommutePro.Domain.Entities
{

    public class Route
    {
        public string RouteId { get; private set; } = string.Empty;
        public string RouteShortName { get; private set; } = string.Empty;
        public string? RouteLongName { get; private set; }
        public string? RouteColor { get; private set; }
        public string? RouteTextColor { get; private set; }
        public byte RouteType { get; private set; } // 2=rail
        public string AgencyId { get; private set; } = string.Empty;
        public DateTime CreatedAt { get; private set; }

        private readonly List<Trip> _trips = new();
        public IReadOnlyCollection<Trip> Trips => _trips.AsReadOnly();

        private Route() { }

        public Route(string routeId, string routeShortName, byte routeType, string agencyId,
                     string? routeLongName = null, string? routeColor = null, string? routeTextColor = null)
        {
            RouteId = routeId ?? throw new ArgumentNullException(nameof(routeId));
            RouteShortName = routeShortName ?? throw new ArgumentNullException(nameof(routeShortName));
            RouteType = routeType;
            AgencyId = agencyId ?? throw new ArgumentNullException(nameof(agencyId));
            RouteLongName = routeLongName;
            RouteColor = routeColor;
            RouteTextColor = routeTextColor;
            CreatedAt = DateTime.UtcNow;
        }
    }
}

