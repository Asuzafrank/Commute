using CommutePro.Application.Common;
using CommutePro.Application.DTOs.Auth;
using CommutePro.Application.Features.Auth.Register.Command;
using CommutePro.Application.Interfaces;
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

namespace CommutePro.Application.Features.Auth.Register.Handler
{
    // Application/Features/Auth/Handlers/RegisterUserHandler.cs
    public class RegisterUserHandler : IRequestHandler<RegisterUserCommand, BaseResponse<AuthResponse>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly IUnitOfWork _unitOfWork;  // ← ADD THIS
        private readonly ILogger<RegisterUserHandler> _logger;

        public RegisterUserHandler(
            UserManager<ApplicationUser> userManager,
            ITokenService tokenService,
            IUnitOfWork unitOfWork,  // ← ADD
            ILogger<RegisterUserHandler> logger)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _unitOfWork = unitOfWork;  // ← ADD
            _logger = logger;
        }

        public async Task<BaseResponse<AuthResponse>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
                return BaseResponse<AuthResponse>.Fail("User with this email already exists");

            var user = ApplicationUser.Create(request.Email, request.UserName);
            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return BaseResponse<AuthResponse>.Fail("Registration failed", errors);
            }

            user.InitializeNotificationPreferences();
            await _userManager.UpdateAsync(user);

            await _unitOfWork.SaveChangesAsync(cancellationToken);  // ← ADD THIS LINE

            var (token, expiresAt) = await _tokenService.GenerateAccessTokenAsync(user);
            var refreshToken = _tokenService.GenerateRefreshToken();

            _logger.LogInformation("User registered successfully: {Email}", request.Email);

            return BaseResponse<AuthResponse>.Ok(new AuthResponse
            {
                Token = token,
                RefreshToken = refreshToken,
                UserId = user.Id.ToString(),
                Email = user.Email ?? request.Email,
                UserName = user.UserName ?? request.UserName,
                ExpiresAt = expiresAt
            }, "Registration successful");
        }
    }
}
