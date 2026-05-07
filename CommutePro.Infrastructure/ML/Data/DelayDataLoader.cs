using CommutePro.Infrastructure.ML.Models;
using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommutePro.Infrastructure.ML.Data
{
    public class DelayDataLoader
    {
        // Load from your training_data.csv (sync version)
        public List<DelayPredictionInput> LoadFromCsvSync(string csvPath, int maxRows = 100000)
        {
            var records = new List<DelayPredictionInput>();

            using var reader = new StreamReader(csvPath);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            // Read header
            csv.Read();
            csv.ReadHeader();

            int rowCount = 0;
            while (csv.Read() && rowCount < maxRows)
            {
                var record = new DelayPredictionInput
                {
                    RouteId = csv.GetField("route_id") ?? "Unknown",
                    HourOfDay = csv.GetField<float>("hour_of_day"),
                    DayOfWeek = csv.GetField<float>("day_of_week"),
                    IsRushHour = csv.GetField<float>("is_rush_hour"),
                    TravelTimeSeconds = csv.GetField<float>("travel_time_seconds")  // ← Changed from DelaySeconds
                };

                records.Add(record);
                rowCount++;
            }

            return records;
        }

        // Async version (if needed)
        public async Task<List<DelayPredictionInput>> LoadFromCsvAsync(string csvPath, int maxRows = 100000)
        {
            var records = new List<DelayPredictionInput>();

            using var reader = new StreamReader(csvPath);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            await csv.ReadAsync();
            csv.ReadHeader();

            int rowCount = 0;
            while (await csv.ReadAsync() && rowCount < maxRows)
            {
                var record = new DelayPredictionInput
                {
                    RouteId = csv.GetField("route_id") ?? "Unknown",
                    HourOfDay = csv.GetField<float>("hour_of_day"),
                    DayOfWeek = csv.GetField<float>("day_of_week"),
                    IsRushHour = csv.GetField<float>("is_rush_hour"),
                    TravelTimeSeconds = csv.GetField<float>("travel_time_seconds")
                };

                records.Add(record);
                rowCount++;
            }

            return records;
        }
    }
}