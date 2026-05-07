using CommutePro.Application.Common;
using CommutePro.Application.DTOs.Alerts;
using CommutePro.Application.DTOs.GTFS;
using CommutePro.Application.Features.Alerts.Queries;
using CommutePro.Application.Interfaces;
using CommutePro.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommutePro.Application.Features.Alerts.Handlers
{
    public class GetAlertByIdHandler : IRequestHandler<GetAlertByIdQuery, BaseResponse<AlertDetailDto>>
    {
        private readonly IGtfsRealtimeClient _realtimeClient;
        private readonly ILogger<GetAlertByIdHandler> _logger;

        public GetAlertByIdHandler(
            IGtfsRealtimeClient realtimeClient,
            ILogger<GetAlertByIdHandler> logger)
        {
            _realtimeClient = realtimeClient;
            _logger = logger;
        }

        public async Task<BaseResponse<AlertDetailDto>> Handle(GetAlertByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // Get all alerts from the realtime feed
                var alertsResponse = await _realtimeClient.GetServiceAlertsAsync(cancellationToken);
                if (alertsResponse == null)
                    return BaseResponse<AlertDetailDto>.Fail("No alerts available");

                // Find the specific alert by ID
                var rawAlert = alertsResponse.Alerts
                    .FirstOrDefault(a => a.Id == request.AlertId);

                if (rawAlert == null)
                {
                    return BaseResponse<AlertDetailDto>.Fail($"Alert {request.AlertId} not found");
                }

                // Transform to AlertDetailDto
                var alertDetail = MapToAlertDetailDto(rawAlert);

                return BaseResponse<AlertDetailDto>.Ok(alertDetail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching alert {AlertId}", request.AlertId);
                return BaseResponse<AlertDetailDto>.Fail("Failed to retrieve alert");
            }
        }

        private AlertDetailDto MapToAlertDetailDto(ServiceAlert rawAlert)
        {
            var alertDetail = new AlertDetailDto
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
                IsActive = IsAlertActive(rawAlert),
                ServiceEffectText = rawAlert.ServiceEffectText,
                ImageUrl = rawAlert.ImageUrl,
                ImageAlternativeText = rawAlert.ImageAlternativeText,
                TimeframeText = rawAlert.TimeframeText,
                RecurrenceText = rawAlert.RecurrenceText,
                CreatedAt = rawAlert.CreatedTimestamp.HasValue
                    ? DateTimeOffset.FromUnixTimeSeconds(rawAlert.CreatedTimestamp.Value).LocalDateTime
                    : null,
                LastModified = rawAlert.LastModifiedTimestamp.HasValue
                    ? DateTimeOffset.FromUnixTimeSeconds(rawAlert.LastModifiedTimestamp.Value).LocalDateTime
                    : null
            };

            return alertDetail;
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
                AlertEffect.AccessibilityIssue => "Accessibility Issue",
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
                AlertEffect.StopMoved => "WARNING",
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
