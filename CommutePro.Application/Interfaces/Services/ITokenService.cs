using CommutePro.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommutePro.Application.Interfaces.Services
{
    public interface ITokenService
    {
        Task<(string token, DateTime expiresAt)> GenerateAccessTokenAsync(ApplicationUser user);
        string GenerateRefreshToken();
    }
}
