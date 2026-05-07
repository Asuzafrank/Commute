using CommutePro.Application.Interfaces.BaseRepository;
using CommutePro.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommutePro.Application.Interfaces.Repositories
{
    public interface IUserRepository : IRepository<ApplicationUser, Guid>
    {
        Task<ApplicationUser?> GetWithFavouritesAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    }
}
