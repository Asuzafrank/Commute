using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommutePro.Application.DTOs.Alerts
{
    public class AlertDetailDto : AlertDto
    {
        public string? ImageUrl { get; set; }
        public string? ImageAlternativeText { get; set; }
        public string? ServiceEffectText { get; set; }
        public string? TimeframeText { get; set; }
        public string? RecurrenceText { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? LastModified { get; set; }
    }
}
