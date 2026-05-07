using CommutePro.Application.Interfaces.BaseRepository;
using CommutePro.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommutePro.Application.Interfaces.Repositories
{
    public interface IAgencyRepository : IRepository<Agency, string>
    {
        Task<Agency?> GetDefaultAgencyAsync(CancellationToken cancellationToken = default);
    }
}
