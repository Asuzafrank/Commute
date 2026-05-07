using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommutePro.Application.DTOs.Alerts
{
    public class AlertDto
    {
        public string Id { get; set; } = string.Empty;
        public string HeaderText { get; set; } = string.Empty;
        public string? DescriptionText { get; set; }
        public string Effect { get; set; } = string.Empty;
        public string? Cause { get; set; }
        public string Severity { get; set; } = string.Empty;
        public List<string> AffectedRoutes { get; set; } = new();
        public List<string> AffectedStops { get; set; } = new();
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string? Url { get; set; }
        public bool IsActive { get; set; }
    }
}
