using CommutePro.Application.Interfaces.ML;
using CommutePro.Infrastructure.ML.Data;
using CommutePro.Infrastructure.ML.Models;
using CommutePro.Infrastructure.ML.Trainer;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace CommutePro.Infrastructure.Services.ML
{
    public class DelayPredictionService : IDelayPredictionService
    {
        private readonly DelayTrainer _trainer;
        private readonly ILogger<DelayPredictionService> _logger;
        private bool _isTrained = false;

        public DelayPredictionService(ILogger<DelayPredictionService> logger)
        {
            _logger = logger;
            _trainer = new DelayTrainer();
        }

        public async Task TrainModelAsync()
        {
            _logger.LogInformation("Initializing ML model...");

            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            var modelPath = Path.Combine(basePath, "Models", "delay_prediction.zip");

            // ✅ PRODUCTION PATH: Load pre-trained model if exists
            if (File.Exists(modelPath))
            {
                _logger.LogInformation($"Loading pre-trained model from {modelPath}");
                _trainer.LoadModel(modelPath);
                _isTrained = true;
                _logger.LogInformation("✅ Model loaded successfully");
                return;
            }

            // ⚠️ DEVELOPMENT PATH: Train only if model doesn't exist (local development)
            _logger.LogWarning("Model file not found. Training new model (development mode)...");

            var csvPath = Path.Combine(basePath, "Data", "ML", "training_data.csv");

            // Try to find CSV in Infrastructure folder (fallback for local dev)
            if (!File.Exists(csvPath))
            {
                var infrastructurePath = Path.GetFullPath(Path.Combine(basePath, "..", "..", "..", "CommutePro.Infrastructure"));
                csvPath = Path.Combine(infrastructurePath, "Data", "ML", "training_data.csv");
            }

            if (!File.Exists(csvPath))
            {
                _logger.LogError($"Training data not found at {csvPath}");
                return;
            }

            var loader = new DelayDataLoader();
            var trainingData = loader.LoadFromCsvSync(csvPath, maxRows: 50000);

            if (trainingData.Count == 0)
            {
                _logger.LogWarning("No training data loaded!");
                return;
            }

            _trainer.Train(trainingData);
            _trainer.SaveModel(modelPath);
            _isTrained = true;

            _logger.LogInformation("✅ Model trained and saved");
        }

        public async Task<float> PredictDelayAsync(string routeId, DateTime departureTime, float scheduledTravelTimeSeconds = 0)
        {
            if (!_isTrained)
            {
                _logger.LogWarning("Model not loaded, returning default");
                return 0;
            }

            var input = new DelayPredictionInput
            {
                RouteId = routeId,
                HourOfDay = departureTime.Hour,
                DayOfWeek = (float)departureTime.DayOfWeek,
                IsRushHour = (departureTime.Hour >= 7 && departureTime.Hour <= 9) ||
                             (departureTime.Hour >= 16 && departureTime.Hour <= 19) ? 1 : 0,
                TravelTimeSeconds = 0
            };

            var predictedTravelTime = _trainer.PredictTravelTime(input);

            float predictedDelay = 0;
            if (scheduledTravelTimeSeconds > 0)
            {
                predictedDelay = Math.Max(0, predictedTravelTime - scheduledTravelTimeSeconds);
            }

            _logger.LogDebug($"Predicted travel time for {routeId} at {departureTime:h:mm tt}: {predictedTravelTime} seconds");
            _logger.LogDebug($"Predicted delay: {predictedDelay} seconds");

            return predictedDelay;
        }

        public async Task<float> PredictTravelTimeAsync(string routeId, DateTime departureTime)
        {
            if (!_isTrained)
            {
                _logger.LogWarning("Model not loaded, returning default");
                return 60;
            }

            var input = new DelayPredictionInput
            {
                RouteId = routeId,
                HourOfDay = departureTime.Hour,
                DayOfWeek = (float)departureTime.DayOfWeek,
                IsRushHour = (departureTime.Hour >= 7 && departureTime.Hour <= 9) ||
                             (departureTime.Hour >= 16 && departureTime.Hour <= 19) ? 1 : 0,
                TravelTimeSeconds = 0
            };

            return _trainer.PredictTravelTime(input);
        }
    }
}