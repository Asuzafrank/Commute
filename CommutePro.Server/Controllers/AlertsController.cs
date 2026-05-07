using CommutePro.Application.Common;
using CommutePro.Application.DTOs.Alerts;
using CommutePro.Application.Features.Alerts.Queries;
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
    public class AlertsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<AlertsController> _logger;

        public AlertsController(IMediator mediator, ILogger<AlertsController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Get all active service alerts
        /// </summary>
        /// <returns>List of active alerts</returns>
        [HttpGet]
        [ProducesResponseType(typeof(BaseResponse<List<AlertDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllAlerts()
        {
            _logger.LogDebug("Getting all active alerts");

            var query = new GetActiveAlertsQuery();
            var result = await _mediator.Send(query);

            return Ok(result);
        }

        /// <summary>
        /// Get alerts for a specific route
        /// </summary>
        /// <param name="routeId">Route ID (e.g., "Red", "Green-B", "CR-Providence")</param>
        /// <returns>List of alerts affecting this route</returns>
        [HttpGet("route/{routeId}")]
        [ProducesResponseType(typeof(BaseResponse<List<AlertDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAlertsByRoute(string routeId)
        {
            _logger.LogDebug("Getting alerts for route: {RouteId}", routeId);

            var query = new GetActiveAlertsQuery { RouteId = routeId };
            var result = await _mediator.Send(query);

            return Ok(result);
        }

        /// <summary>
        /// Get alerts for a specific station
        /// </summary>
        /// <param name="stopId">Station ID (e.g., "place-north", "place-south")</param>
        /// <returns>List of alerts affecting this station</returns>
        [HttpGet("station/{stopId}")]
        [ProducesResponseType(typeof(BaseResponse<List<AlertDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAlertsByStation(string stopId)
        {
            _logger.LogDebug("Getting alerts for station: {StopId}", stopId);

            var query = new GetActiveAlertsQuery { StopId = stopId };
            var result = await _mediator.Send(query);

            return Ok(result);
        }

        /// <summary>
        /// Get alerts for user's favourite stations and routes
        /// </summary>
        /// <returns>List of alerts relevant to user's favourites</returns>
        [HttpGet("favourites")]
        [Authorize]
        [ProducesResponseType(typeof(BaseResponse<List<AlertDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAlertsForFavourites(
            [FromServices] ICurrentUserService currentUser,
            [FromServices] IFavouriteRepository favouriteRepository,
            [FromServices] ITripRepository tripRepository)
        {
            var userId = currentUser.UserId;
            if (!userId.HasValue)
                return Unauthorized(new { message = "User not authenticated" });

            // Get user's favourite stations
            var favourites = await favouriteRepository.GetByUserIdAsync(userId.Value);

            if (!favourites.Any())
            {
                return Ok(BaseResponse<List<AlertDto>>.Ok(new List<AlertDto>()));
            }

            // Get unique route IDs from user's favourite trips (optional - for route-based alerts)
            // This is simplified - you can expand to include routes from favourites

            var stopIds = favourites.Select(f => f.StopId).Distinct().ToList();

            _logger.LogDebug("Getting alerts for {Count} favourite stations for user {UserId}",
                stopIds.Count, userId);

            // Get alerts for all favourite stations
            var allAlerts = new List<AlertDto>();

            foreach (var stopId in stopIds)
            {
                var query = new GetActiveAlertsQuery { StopId = stopId };
                var result = await _mediator.Send(query);

                if (result.Success && result.Data != null)
                {
                    allAlerts.AddRange(result.Data);
                }
            }

            // Remove duplicates by ID
            var uniqueAlerts = allAlerts
                .GroupBy(a => a.Id)
                .Select(g => g.First())
                .OrderByDescending(a => a.StartTime)
                .ToList();

            return Ok(BaseResponse<List<AlertDto>>.Ok(uniqueAlerts));
        }

        /// <summary>
        /// Get alert by ID
        /// </summary>
        /// <param name="alertId">Alert ID</param>
        /// <returns>Alert details</returns>
        [HttpGet("{alertId}")]
        [ProducesResponseType(typeof(BaseResponse<AlertDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAlertById(string alertId)
        {
            _logger.LogDebug("Getting alert by ID: {AlertId}", alertId);

            var query = new GetAlertByIdQuery { AlertId = alertId };
            var result = await _mediator.Send(query);

            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }
    }
}
