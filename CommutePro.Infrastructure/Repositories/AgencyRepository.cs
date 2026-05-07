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
    public class AgencyRepository : GenericRepository<Agency, string>, IAgencyRepository
    {
        public AgencyRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Agency?> GetDefaultAgencyAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Agencies
                .FirstOrDefaultAsync(a => a.AgencyId == "MBTA", cancellationToken);
        }
    }
}
