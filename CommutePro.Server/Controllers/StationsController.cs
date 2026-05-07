using CommutePro.Application.Common;
using CommutePro.Application.DTOs.Stations;
using CommutePro.Application.Features.Stations.Queries;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CommutePro.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StationsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<StationsController> _logger;

        public StationsController(IMediator mediator, ILogger<StationsController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Search stations by name or ID
        /// </summary>
        /// <param name="query">Search query (minimum 2 characters)</param>
        /// <param name="limit">Maximum number of results (default 20)</param>
        /// <returns>List of matching stations</returns>
        [HttpGet("search")]
        [ProducesResponseType(typeof(BaseResponse<List<StationDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> SearchStations(
            [FromQuery] string q,
            [FromQuery] int limit = 20)
        {
            if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
            {
                return Ok(BaseResponse<List<StationDto>>.Ok(new List<StationDto>()));
            }

            _logger.LogDebug("Searching stations with query: {Query}, limit: {Limit}", q, limit);

            var query = new SearchStationsQuery
            {
                Query = q,
                Limit = Math.Min(limit, 50) // Cap at 50 results
            };

            var result = await _mediator.Send(query);

            return Ok(result);
        }

        /// <summary>
        /// Get station by ID
        /// </summary>
        /// <param name="stopId">Station ID (e.g., "place-north", "place-south")</param>
        /// <returns>Station details</returns>
        [HttpGet("{stopId}")]
        [ProducesResponseType(typeof(BaseResponse<StationDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<StationDto>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetStationById(string stopId)
        {
            _logger.LogDebug("Getting station by ID: {StopId}", stopId);

            var query = new GetStationByIdQuery { StopId = stopId };
            var result = await _mediator.Send(query);

            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        /// <summary>
        /// Get nearby stations (optional - for future implementation)
        /// </summary>
        /// <param name="lat">Latitude</param>
        /// <param name="lon">Longitude</param>
        /// <param name="radius">Radius in meters (default 1000)</param>
        /// <param name="limit">Maximum number of results (default 20)</param>
        [HttpGet("nearby")]
        [ProducesResponseType(StatusCodes.Status501NotImplemented)]
        public IActionResult GetNearbyStations(
            [FromQuery] decimal lat,
            [FromQuery] decimal lon,
            [FromQuery] int radius = 1000,
            [FromQuery] int limit = 20)
        {
            // TODO: Implement nearby stations using PostGIS when ready
            return StatusCode(501, new { message = "Nearby stations feature coming soon" });
        }

        /// <summary>
        /// Get all stations (paginated) - admin only
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status501NotImplemented)]
        public IActionResult GetAllStations(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            // TODO: Implement paginated stations list (admin only)
            return StatusCode(501, new { message = "Coming soon" });
        }
    }
}
