using CommutePro.Application.Common;
using CommutePro.Application.DTOs.Auth;
using CommutePro.Application.Features.Auth.Login.Command;
using CommutePro.Application.Interfaces.Services;
using CommutePro.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommutePro.Application.Features.Auth.Login.Handler
{
    // Application/Features/Auth/Handlers/LoginUserHandler.cs
    public class LoginUserHandler : IRequestHandler<LoginUserCommand, BaseResponse<AuthResponse>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly ILogger<LoginUserHandler> _logger;

        public LoginUserHandler(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ITokenService tokenService,
            ILogger<LoginUserHandler> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _logger = logger;
        }

        public async Task<BaseResponse<AuthResponse>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
        {
            // Find user by email
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                return BaseResponse<AuthResponse>.Fail("Invalid email or password");

            // Check password
            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);

            if (!result.Succeeded)
                return BaseResponse<AuthResponse>.Fail("Invalid email or password");

            // Generate token (async)
            var (token, expiresAt) = await _tokenService.GenerateAccessTokenAsync(user);
            var refreshToken = _tokenService.GenerateRefreshToken();

            _logger.LogInformation("User logged in successfully: {Email}", request.Email);

            return BaseResponse<AuthResponse>.Ok(new AuthResponse
            {
                Token = token,
                RefreshToken = refreshToken,
                UserId = user.Id.ToString(),
                Email = user.Email ?? request.Email,
                UserName = user.UserName ?? request.Email,
                ExpiresAt = expiresAt
            }, "Login successful");
        }
    }
}
