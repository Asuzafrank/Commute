using CommutePro.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace CommutePro.Infrastructure.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid? UserId
        {
            get
            {
                // Try both "userId" and "sub" (JwtRegisteredClaimNames.Sub)
                var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("userId")
                    ?? _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)
                    ?? _httpContextAccessor.HttpContext?.User?.FindFirst("sub");

                if (userIdClaim == null || string.IsNullOrEmpty(userIdClaim.Value))
                    return null;

                return Guid.TryParse(userIdClaim.Value, out var userId) ? userId : null;
            }
        }

        public string? Email
        {
            get
            {
                // Try both "email" and ClaimTypes.Email
                return _httpContextAccessor.HttpContext?.User?.FindFirst("email")?.Value
                    ?? _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;
            }
        }

        public string? UserName
        {
            get
            {
                // Try both "name" and ClaimTypes.Name
                return _httpContextAccessor.HttpContext?.User?.FindFirst("name")?.Value
                    ?? _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value;
            }
        }

        public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
    }
}

