using CommutePro.Application.DTOs.Arrivals;
using CommutePro.Application.Features.Arrivals.Queries;
using CommutePro.Application.Interfaces.Repositories;
using CommutePro.Application.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CommutePro.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArrivalsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ArrivalsController> _logger;

        public ArrivalsController(IMediator mediator, ILogger<ArrivalsController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Get real-time arrivals for a station
        /// </summary>
        /// <param name="stopId">Station ID (e.g., "place-north", "place-south")</param>
        /// <returns>List of arriving trains with countdown timers and delay info</returns>
        [HttpGet("{stopId}")]
        [ProducesResponseType(typeof(ArrivalsResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetArrivals(string stopId)
        {
            _logger.LogDebug("Getting arrivals for station: {StopId}", stopId);

            var query = new GetStationArrivalsQuery { StopId = stopId };
            var result = await _mediator.Send(query);

            // Check if station was found
            if (result.StationName == "Unknown Station" && !result.Arrivals.Any())
            {
                return NotFound(new { message = $"Station {stopId} not found" });
            }

            return Ok(result);
        }

        /// <summary>
        /// Get real-time arrivals for multiple stations (batch)
        /// </summary>
        /// <param name="request">List of station IDs</param>
        /// <returns>Dictionary of station ID to arrivals</returns>
        [HttpPost("batch")]
        [ProducesResponseType(typeof(Dictionary<string, ArrivalsResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetBatchArrivals([FromBody] BatchArrivalsRequest request)
        {
            if (request.StopIds == null || !request.StopIds.Any())
            {
                return BadRequest(new { message = "At least one stop ID is required" });
            }

            // Limit to 20 stations per request to prevent abuse
            var stopIds = request.StopIds.Take(20).Distinct();

            _logger.LogDebug("Getting batch arrivals for {Count} stations", stopIds.Count());

            var results = new Dictionary<string, ArrivalsResponseDto>();

            foreach (var stopId in stopIds)
            {
                var query = new GetStationArrivalsQuery { StopId = stopId };
                var result = await _mediator.Send(query);
                results[stopId] = result;
            }

            return Ok(results);
        }

        /// <summary>
        /// Get arrivals for user's favourite stations
        /// </summary>
        /// <returns>Dictionary of station ID to arrivals</returns>
        [HttpGet("favourites")]
        [Authorize] // Requires authentication
        [ProducesResponseType(typeof(Dictionary<string, ArrivalsResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetFavouriteArrivals(
            [FromServices] ICurrentUserService currentUser,
            [FromServices] IFavouriteRepository favouriteRepository)
        {
            var userId = currentUser.UserId;
            if (!userId.HasValue)
                return Unauthorized(new { message = "User not authenticated" });

            // Get user's favourite stations
            var favourites = await favouriteRepository.GetByUserIdAsync(userId.Value);

            if (!favourites.Any())
            {
                return Ok(new Dictionary<string, ArrivalsResponseDto>());
            }

            var stopIds = favourites.OrderBy(f => f.SortOrder).Select(f => f.StopId);

            _logger.LogDebug("Getting arrivals for {Count} favourite stations for user {UserId}",
                stopIds.Count(), userId);

            var results = new Dictionary<string, ArrivalsResponseDto>();

            foreach (var stopId in stopIds)
            {
                var query = new GetStationArrivalsQuery { StopId = stopId };
                var result = await _mediator.Send(query);
                results[stopId] = result;
            }

            return Ok(results);
        }
    }

    // Request DTOs
    public class BatchArrivalsRequest
    {
        public List<string> StopIds { get; set; } = new();
    }
}
