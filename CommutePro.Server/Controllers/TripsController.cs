using CommutePro.Application.Common;
using CommutePro.Application.DTOs.Trips;
using CommutePro.Application.Features.Trips.Queries;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CommutePro.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TripsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<TripsController> _logger;

        public TripsController(IMediator mediator, ILogger<TripsController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpGet("{tripId}")]
        [ProducesResponseType(typeof(BaseResponse<TripDetailsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTripDetails(string tripId)
        {
            _logger.LogDebug("Getting trip details for: {TripId}", tripId);

            var query = new GetTripDetailsQuery { TripId = tripId };
            var result = await _mediator.Send(query);

            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        [HttpGet("plan-direct")]
        [ProducesResponseType(typeof(BaseResponse<TripPlanResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> PlanDirectTrip(
       [FromQuery] string from,
       [FromQuery] string to,
       [FromQuery] DateTime? departureTime = null)
        {
            var query = new PlanDirectTripQuery
            {
                FromStopId = from,
                ToStopId = to,
                DepartureTime = departureTime ?? DateTime.Now
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }
    }
}
