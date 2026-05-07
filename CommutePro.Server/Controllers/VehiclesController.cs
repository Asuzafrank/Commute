using CommutePro.Application.Common;
using CommutePro.Application.DTOs.GTFS;
using CommutePro.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CommutePro.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VehiclesController : ControllerBase
    {
        private readonly IRealtimeCacheService _cache;

        public VehiclesController(IRealtimeCacheService cache)
        {
            _cache = cache;
        }

        [HttpGet]
        public IActionResult GetVehiclePositions()
        {
            var positions = _cache.GetVehiclePositions();

            var vehicles = positions?.Vehicles ?? new List<VehiclePosition>();

            var result = vehicles.Select(v => new
            {
                id = v.TripId,
                tripId = v.TripId,
                routeId = v.RouteId,
                latitude = v.Latitude,
                longitude = v.Longitude,
                bearing = v.Bearing ?? 0,
                currentStatus = v.CurrentStatus.ToString(),
                timestamp = v.Timestamp
            });

            return Ok(BaseResponse<object>.Ok(result));
        }
    }
}
