using CommutePro.Application.Interfaces.ML;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CommutePro.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestingController : ControllerBase
    {
        private readonly IDelayPredictionService _mlService;
        private readonly ILogger<TestController> _logger;

        public TestingController(IDelayPredictionService mlService, ILogger<TestController> logger)
        {
            _mlService = mlService;
            _logger = logger;
        }

        [HttpGet("test-ml")]
        public async Task<IActionResult> TestML()
        {
            var predictions = new List<object>();

            var testScenarios = new[]
            {
            new { Route = "Red", Hour = 8, Day = 1, IsRush = 1 },
            new { Route = "Orange", Hour = 17, Day = 5, IsRush = 1 },
            new { Route = "Blue", Hour = 14, Day = 3, IsRush = 0 },
            new { Route = "Green", Hour = 9, Day = 2, IsRush = 1 },
            new { Route = "109", Hour = 8, Day = 1, IsRush = 1 }  // Bus route
        };

            foreach (var scenario in testScenarios)
            {
                var prediction = await _mlService.PredictDelayAsync(
                    scenario.Route,
                    new DateTime(2026, 4, 26, scenario.Hour, 0, 0),
                    60  // scheduled travel time of 60 seconds
                );

                predictions.Add(new
                {
                    route = scenario.Route,
                    hour = scenario.Hour,
                    dayOfWeek = scenario.Day,
                    isRushHour = scenario.IsRush == 1,
                    predictedDelaySeconds = prediction,
                    predictedDelayMinutes = Math.Round(prediction / 60, 1)
                });
            }

            return Ok(new
            {
                modelTrained = true,
                predictions = predictions
            });
        }
    }
}
