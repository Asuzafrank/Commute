using CommutePro.Infrastructure.ML.Data;
using CommutePro.Infrastructure.ML.Trainer;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommutePro.Infrastructure.ML.Training
{
    public static class ModelTrainer
    {
        public static void TrainAndSaveModel()
        {
            Console.WriteLine("========================================");
            Console.WriteLine("ML Model Training");
            Console.WriteLine("========================================");

            // Path to your training data
            var csvPath = @"C:\Users\lenovo\source\repos\CommutePro\CommutePro.Infrastructure\Data\ML\training_data.csv";

            if (!File.Exists(csvPath))
            {
                Console.WriteLine($"ERROR: Training data not found at {csvPath}");
                return;
            }

            Console.WriteLine($"Loading training data from: {csvPath}");
            var loader = new DelayDataLoader();
            var trainingData = loader.LoadFromCsvSync(csvPath, maxRows: 100000);

            Console.WriteLine($"Loaded {trainingData.Count} records");

            Console.WriteLine("Training model...");
            var trainer = new DelayTrainer();
            trainer.Train(trainingData);

            // Save to API project's Models folder
            var modelPath = @"C:\Users\lenovo\source\repos\CommutePro\CommutePro.Server\Models\delay_prediction.zip";
            Directory.CreateDirectory(Path.GetDirectoryName(modelPath));
            trainer.SaveModel(modelPath);

            Console.WriteLine($"✅ Model saved to {modelPath}");
            Console.WriteLine("========================================");
        }

    }
}
