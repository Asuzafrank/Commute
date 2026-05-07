using CommutePro.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommutePro.Application.DTOs.GTFS
{
    public class ServiceAlert
    {
        public string Id { get; set; } = string.Empty;
        public List<InformedEntity> InformedEntities { get; set; } = new();
        public AlertCause Cause { get; set; }
        public AlertEffect Effect { get; set; }
        public string? HeaderText { get; set; }
        public string? DescriptionText { get; set; }
        public string? ServiceEffectText { get; set; } 
        public string? Url { get; set; }
        public string? ImageUrl { get; set; }          
        public string? ImageAlternativeText { get; set; }
        public string? TimeframeText { get; set; }       
        public string? RecurrenceText { get; set; }      
        public long? StartTime { get; set; }
        public long? EndTime { get; set; }
        public int? SeverityLevel { get; set; }
        public long? CreatedTimestamp { get; set; }      
        public long? LastModifiedTimestamp { get; set; }
    }
}
