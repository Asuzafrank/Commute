using CommutePro.Infrastructure.ML.Models;
using Microsoft.ML;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CommutePro.Infrastructure.ML.Trainer
{
    public class DelayTrainer
    {
        private readonly MLContext _mlContext;
        private ITransformer? _model;

        public DelayTrainer()
        {
            _mlContext = new MLContext(seed: 42);
        }

        public void Train(List<DelayPredictionInput> trainingData)
        {
            // Convert list to IDataView
            var dataView = _mlContext.Data.LoadFromEnumerable(trainingData);

            // Define the pipeline - FIXED (no named parameter)
            var pipeline = _mlContext.Transforms
                .Categorical.OneHotEncoding("RouteIdEncoded", nameof(DelayPredictionInput.RouteId))
                .Append(_mlContext.Transforms.Concatenate("Features",
                    nameof(DelayPredictionInput.HourOfDay),
                    nameof(DelayPredictionInput.DayOfWeek),
                    nameof(DelayPredictionInput.IsRushHour),
                    "RouteIdEncoded"))
                .Append(_mlContext.Regression.Trainers.FastTree(
                    labelColumnName: nameof(DelayPredictionInput.TravelTimeSeconds),
                    featureColumnName: "Features"));

            // Train the model
            _model = pipeline.Fit(dataView);

            // Evaluate model
            var predictions = _model.Transform(dataView);
            var metrics = _mlContext.Regression.Evaluate(predictions, labelColumnName: nameof(DelayPredictionInput.TravelTimeSeconds));

            Console.WriteLine($"R² Score: {metrics.RSquared:0.00}");
            Console.WriteLine($"Root Mean Squared Error: {metrics.RootMeanSquaredError:0.00}");
        }

        public float PredictTravelTime(DelayPredictionInput input)
        {
            if (_model == null)
                throw new InvalidOperationException("Model not trained yet");

            var predictionEngine = _mlContext.Model.CreatePredictionEngine<DelayPredictionInput, DelayPredictionOutput>(_model);
            var prediction = predictionEngine.Predict(input);

            return Math.Max(0, prediction.PredictedTravelTimeSeconds);
        }

        public void SaveModel(string path)
        {
            if (_model != null)
            {
                _mlContext.Model.Save(_model, null, path);
            }
        }

        public void LoadModel(string path)
        {
            _model = _mlContext.Model.Load(path, out _);
        }
    }
}