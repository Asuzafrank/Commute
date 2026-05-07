using CommutePro.Application.Common;
using CommutePro.Application.DTOs.Favourites;
using CommutePro.Application.Features.Favourites.Commands;
using CommutePro.Application.Features.Favourites.Queries;
using CommutePro.Application.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CommutePro.Api.Controllers
{
    [Route("api/favourites")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class FavouritesController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ICurrentUserService _currentUser;
        private readonly ILogger<FavouritesController> _logger;

        public FavouritesController(
            IMediator mediator,
            ICurrentUserService currentUser,
            ILogger<FavouritesController> logger)
        {
            _mediator = mediator;
            _currentUser = currentUser;
            _logger = logger;
        }

        /// <summary>
        /// Get all favourite stations for the current user
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(BaseResponse<List<FavouriteStationDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetFavourites()
        {
            var userId = _currentUser.UserId;
            if (!userId.HasValue)
                return Unauthorized(new { message = "User not authenticated" });

            _logger.LogDebug("Getting favourites for user {UserId}", userId);

            var query = new GetFavouritesQuery { UserId = userId.Value };
            var result = await _mediator.Send(query);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Add a station to favourites
        /// </summary>
        /// <param name="request">Station ID to add</param>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResponse<FavouriteStationDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<FavouriteStationDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> AddFavourite([FromBody] AddFavouriteRequest request)
        {
            var userId = _currentUser.UserId;
            if (!userId.HasValue)
                return Unauthorized(new { message = "User not authenticated" });

            if (string.IsNullOrWhiteSpace(request.StopId))
                return BadRequest(new { message = "StopId is required" });

            _logger.LogInformation("User {UserId} adding favourite station {StopId}", userId, request.StopId);

            var command = new AddFavouriteCommand
            {
                UserId = userId.Value,
                StopId = request.StopId
            };

            var result = await _mediator.Send(command);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Remove a station from favourites
        /// </summary>
        /// <param name="favouriteId">The favourite ID to remove</param>
        [HttpDelete("{favouriteId}")]
        [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RemoveFavourite(Guid favouriteId)
        {
            var userId = _currentUser.UserId;
            if (!userId.HasValue)
                return Unauthorized(new { message = "User not authenticated" });

            _logger.LogInformation("User {UserId} removing favourite {FavouriteId}", userId, favouriteId);

            var command = new RemoveFavouriteCommand
            {
                UserId = userId.Value,
                FavouriteId = favouriteId
            };

            var result = await _mediator.Send(command);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Reorder favourites (drag and drop)
        /// </summary>
        /// <param name="request">Dictionary of favourite ID to new sort order</param>
        [HttpPost("reorder")]
        [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ReorderFavourites([FromBody] ReorderFavouritesRequest request)
        {
            var userId = _currentUser.UserId;
            if (!userId.HasValue)
                return Unauthorized(new { message = "User not authenticated" });

            if (request.NewOrder == null || !request.NewOrder.Any())
                return BadRequest(new { message = "New order is required" });

            _logger.LogInformation("User {UserId} reordering favourites", userId);

            var command = new ReorderFavouritesCommand
            {
                UserId = userId.Value,
                NewOrder = request.NewOrder
            };

            var result = await _mediator.Send(command);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Check if a station is in user's favourites
        /// </summary>
        /// <param name="stopId">Station ID to check</param>
        [HttpGet("check/{stopId}")]
        [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> IsFavourite(string stopId)
        {
            var userId = _currentUser.UserId;
            if (!userId.HasValue)
                return Unauthorized(new { message = "User not authenticated" });

            var query = new CheckFavouriteQuery { UserId = userId.Value, StopId = stopId };
            var result = await _mediator.Send(query);

            return Ok(result);
        }
        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok("Works");
        }
    }


    // Request DTOs
    public class AddFavouriteRequest
    {
        public string StopId { get; set; } = string.Empty;
    }

    public class ReorderFavouritesRequest
    {
        public Dictionary<Guid, int> NewOrder { get; set; } = new();
    }
}
