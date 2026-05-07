using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommutePro.Application.DTOs.Favourites
{
    public class FavouriteStationDto
    {
        public Guid Id { get; set; }
        public string StopId { get; set; } = string.Empty;
        public string StopName { get; set; } = string.Empty;
        public string? PlatformCode { get; set; }
        public int SortOrder { get; set; }
    }
}
