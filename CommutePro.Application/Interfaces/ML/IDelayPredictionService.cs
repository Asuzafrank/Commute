using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommutePro.Application.Interfaces.ML
{
    public interface IDelayPredictionService
    {
        Task TrainModelAsync();
        Task<float> PredictDelayAsync(string routeId, DateTime departureTime, float scheduledTravelTimeSeconds = 0);
        Task<float> PredictTravelTimeAsync(string routeId, DateTime departureTime);
    }
}
