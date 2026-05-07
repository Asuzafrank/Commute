using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommutePro.Domain.Entities
{
    public class Agency
    {
        public string AgencyId { get; private set; } = string.Empty;
        public string AgencyName { get; private set; } = string.Empty;
        public string? AgencyUrl { get; private set; }
        public string AgencyTimezone { get; private set; } = string.Empty;
        public string? AgencyLang { get; private set; }
        public string? AgencyPhone { get; private set; }
        public DateTime CreatedAt { get; private set; }

        private Agency() { }

        public Agency(string agencyId, string agencyName, string agencyTimezone,
                      string? agencyUrl = null, string? agencyLang = null, string? agencyPhone = null)
        {
            AgencyId = agencyId;
            AgencyName = agencyName;
            AgencyTimezone = agencyTimezone;
            AgencyUrl = agencyUrl;
            AgencyLang = agencyLang;
            AgencyPhone = agencyPhone;
            CreatedAt = DateTime.UtcNow;
        }
    }
}
