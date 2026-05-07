using CommutePro.Application.Common;
using CommutePro.Application.DTOs.Auth;
using CommutePro.Application.Features.Auth.Login.Command;
using CommutePro.Application.Features.Auth.Register.Command;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CommutePro.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IMediator mediator, ILogger<AuthController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Register a new user
        /// </summary>
        /// <param name="command">Registration details</param>
        /// <returns>JWT token and user info</returns>
        [HttpPost("register")]
        [ProducesResponseType(typeof(BaseResponse<AuthResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<AuthResponse>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterUserCommand command)
        {
            _logger.LogInformation("Register attempt for email: {Email}", command.Email);

            var result = await _mediator.Send(command);

            if (!result.Success)
            {
                _logger.LogWarning("Registration failed for {Email}: {Message}", command.Email, result.Message);
                return BadRequest(result);
            }

            _logger.LogInformation("User registered successfully: {Email}", command.Email);
            return Ok(result);
        }

        /// <summary>
        /// Login existing user
        /// </summary>
        /// <param name="command">Login credentials</param>
        /// <returns>JWT token and user info</returns>
        [HttpPost("login")]
        [ProducesResponseType(typeof(BaseResponse<AuthResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<AuthResponse>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginUserCommand command)
        {
            _logger.LogInformation("Login attempt for email: {Email}", command.Email);

            var result = await _mediator.Send(command);

            if (!result.Success)
            {
                _logger.LogWarning("Login failed for {Email}: {Message}", command.Email, result.Message);
                return Unauthorized(result);
            }

            _logger.LogInformation("User logged in successfully: {Email}", command.Email);
            return Ok(result);
        }

        /// <summary>
        /// Refresh token (optional - for future implementation)
        /// </summary>
        [HttpPost("refresh")]
        [ProducesResponseType(StatusCodes.Status501NotImplemented)]
        public IActionResult RefreshToken()
        {
            // TODO: Implement refresh token functionality
            return StatusCode(501, new { message = "Refresh token not implemented yet" });
        }

        /// <summary>
        /// Logout (optional - client-side token removal only)
        /// </summary>
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Logout()
        {
            // For JWT, logout is handled client-side by removing the token
            // This endpoint exists for completeness
            return Ok(new { message = "Logout successful. Please remove token from client." });
        }
    }
}
