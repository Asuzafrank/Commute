using CommutePro.Application.Common;
using CommutePro.Application.DTOs.Alerts;
using CommutePro.Application.DTOs.GTFS;
using CommutePro.Application.Features.Alerts.Queries;
using CommutePro.Application.Interfaces;
using CommutePro.Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommutePro.Application.Features.Alerts.Handlers
{
    public class GetActiveAlertsHandler : IRequestHandler<GetActiveAlertsQuery, BaseResponse<List<AlertDto>>>
    {
        private readonly IGtfsRealtimeClient _realtimeClient;

        public GetActiveAlertsHandler(IGtfsRealtimeClient realtimeClient)
        {
            _realtimeClient = realtimeClient;
        }
        public async Task<BaseResponse<List<AlertDto>>> Handle(GetActiveAlertsQuery request, CancellationToken cancellationToken)
        {
            // Get raw GTFS alerts
            var rawAlerts = await _realtimeClient.GetServiceAlertsAsync(cancellationToken);

            var alerts = new List<AlertDto>();
            if (rawAlerts != null)
            {

                foreach (var rawAlert in rawAlerts.Alerts)
                {
                    // Filter by route or stop if specified
                    if (!string.IsNullOrEmpty(request.RouteId))
                    {
                        if (!rawAlert.InformedEntities.Any(e => e.RouteId == request.RouteId))
                            continue;
                    }

                    if (!string.IsNullOrEmpty(request.StopId))
                    {
                        if (!rawAlert.InformedEntities.Any(e => e.StopId == request.StopId))
                            continue;
                    }

                    // Transform to frontend DTO
                    var dto = new AlertDto
                    {
                        Id = rawAlert.Id,
                        HeaderText = rawAlert.HeaderText ?? "Service Alert",
                        DescriptionText = rawAlert.DescriptionText,
                        Effect = FormatEffect(rawAlert.Effect),
                        Cause = FormatCause(rawAlert.Cause),
                        Severity = GetSeverity(rawAlert),
                        AffectedRoutes = rawAlert.InformedEntities
                            .Where(e => !string.IsNullOrEmpty(e.RouteId))
                            .Select(e => e.RouteId!)
                            .Distinct()
                            .ToList(),
                        AffectedStops = rawAlert.InformedEntities
                            .Where(e => !string.IsNullOrEmpty(e.StopId))
                            .Select(e => e.StopId!)
                            .Distinct()
                            .ToList(),
                        StartTime = rawAlert.StartTime.HasValue
                            ? DateTimeOffset.FromUnixTimeSeconds(rawAlert.StartTime.Value).LocalDateTime
                            : null,
                        EndTime = rawAlert.EndTime.HasValue
                            ? DateTimeOffset.FromUnixTimeSeconds(rawAlert.EndTime.Value).LocalDateTime
                            : null,
                        Url = rawAlert.Url,
                        IsActive = IsAlertActive(rawAlert)
                    };

                    alerts.Add(dto);
                }
            }

            return BaseResponse<List<AlertDto>>.Ok(alerts.OrderByDescending(a => a.StartTime).ToList());
        }

        private string FormatEffect(AlertEffect effect)
        {
            return effect switch
            {
                AlertEffect.NoService => "No Service",
                AlertEffect.ReducedService => "Reduced Service",
                AlertEffect.SignificantDelays => "Significant Delays",
                AlertEffect.Detour => "Detour",
                AlertEffect.AdditionalService => "Extra Service",
                AlertEffect.ModifiedService => "Modified Service",
                AlertEffect.StopMoved => "Stop Relocated",
                _ => "Service Change"
            };
        }

        private string? FormatCause(AlertCause cause)
        {
            return cause switch
            {
                AlertCause.Accident => "Accident",
                AlertCause.Construction => "Construction",
                AlertCause.Maintenance => "Scheduled Maintenance",
                AlertCause.Weather => "Weather",
                AlertCause.TechnicalProblem => "Technical Issue",
                AlertCause.PoliceActivity => "Police Activity",
                AlertCause.Strike => "Strike",
                AlertCause.MedicalEmergency => "Medical Emergency",
                AlertCause.OtherCause => "Service Issue",
                _ => null
            };
        }

        private string GetSeverity(ServiceAlert alert)
        {
            // Use severity level if provided, otherwise infer from effect
            if (alert.SeverityLevel.HasValue)
            {
                return alert.SeverityLevel.Value switch
                {
                    1 => "INFO",
                    2 => "WARNING",
                    3 => "SEVERE",
                    _ => "INFO"
                };
            }

            return alert.Effect switch
            {
                AlertEffect.NoService => "SEVERE",
                AlertEffect.SignificantDelays => "SEVERE",
                AlertEffect.ReducedService => "WARNING",
                AlertEffect.Detour => "WARNING",
                _ => "INFO"
            };
        }

        private bool IsAlertActive(ServiceAlert alert)
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            // If no start time, assume active
            if (!alert.StartTime.HasValue)
                return true;

            // Not started yet
            if (now < alert.StartTime.Value)
                return false;

            // Has end time and expired
            if (alert.EndTime.HasValue && now > alert.EndTime.Value)
                return false;

            return true;
        }
    }
}
