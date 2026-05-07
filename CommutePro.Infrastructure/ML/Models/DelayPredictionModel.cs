using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommutePro.Infrastructure.ML.Models
{
    public class DelayPredictionInput
    {
        [LoadColumn(0)]
        public string RouteId { get; set; } = string.Empty;

        [LoadColumn(1)]
        public float HourOfDay { get; set; }

        [LoadColumn(2)]
        public float DayOfWeek { get; set; }

        [LoadColumn(3)]
        public float IsRushHour { get; set; }

        [LoadColumn(4)]  // This is what we predict
        public float TravelTimeSeconds { get; set; }  
    }

    public class DelayPredictionOutput
    {
        [ColumnName("Score")]
        public float PredictedTravelTimeSeconds { get; set; }  
    }
}
