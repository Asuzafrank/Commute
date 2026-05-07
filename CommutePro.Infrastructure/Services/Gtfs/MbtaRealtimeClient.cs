using CommutePro.Application.DTOs.GTFS;
using CommutePro.Application.Interfaces;
using CommutePro.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CommutePro.Infrastructure.Services.Gtfs
{
    public class MbtaRealtimeClient : IGtfsRealtimeClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<MbtaRealtimeClient> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public MbtaRealtimeClient(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<MbtaRealtimeClient> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        public async Task<ServiceAlertResponse?> GetServiceAlertsAsync(CancellationToken cancellationToken = default)
        {
            var url = _configuration["Gtfs:ServiceAlertUrl"];
            if (string.IsNullOrEmpty(url))
            {
                _logger.LogWarning("ServiceAlert URL not configured");
                return new ServiceAlertResponse();
            }

            try
            {
                var response = await _httpClient.GetStringAsync(url, cancellationToken);
                var mbtaData = JsonSerializer.Deserialize<MbtaServiceAlertResponse>(response, _jsonOptions);

                return MapToServiceAlertResponse(mbtaData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch ServiceAlerts from {Url}", url);
                return new ServiceAlertResponse();
            }
        }

        public async Task<TripUpdateResponse?> GetTripUpdatesAsync(CancellationToken cancellationToken = default)
        {
            var url = _configuration["Gtfs:TripUpdateUrl"];
            if (string.IsNullOrEmpty(url))
            {
                _logger.LogWarning("TripUpdate URL not configured");
                return new TripUpdateResponse();
            }

            try
            {
                var response = await _httpClient.GetStringAsync(url, cancellationToken);
                var mbtaData = JsonSerializer.Deserialize<MbtaTripUpdateResponse>(response, _jsonOptions);

                return MapToTripUpdateResponse(mbtaData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch TripUpdates from {Url}", url);
                return new TripUpdateResponse();
            }
        }

        public async Task<VehiclePositionResponse?> GetVehiclePositionsAsync(CancellationToken cancellationToken = default)
        {
            var url = _configuration["Gtfs:VehiclePositionUrl"];
            if (string.IsNullOrEmpty(url))
            {
                _logger.LogWarning("VehiclePosition URL not configured");
                return new VehiclePositionResponse();
            }

            try
            {
                var response = await _httpClient.GetStringAsync(url, cancellationToken);
                var mbtaData = JsonSerializer.Deserialize<MbtaVehiclePositionResponse>(response, _jsonOptions);

                return MapToVehiclePositionResponse(mbtaData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch VehiclePositions from {Url}", url);
                return new VehiclePositionResponse();
            }
        }

        #region Mapping Methods

        private TripUpdateResponse MapToTripUpdateResponse(MbtaTripUpdateResponse? mbtaData)
        {
            var response = new TripUpdateResponse
            {
                Timestamp = mbtaData?.Header?.Timestamp ?? DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                TripUpdates = new List<TripUpdate>()
            };

            if (mbtaData?.Entity == null)
            {
                _logger.LogWarning("MapToTripUpdateResponse: mbtaData or Entity is null");
                return response;
            }

            _logger.LogInformation("MapToTripUpdateResponse: Processing {Count} entities", mbtaData.Entity.Count);

            foreach (var entity in mbtaData.Entity)
            {
                if (entity.TripUpdate == null)
                {
                    _logger.LogDebug("Entity {Id} has null TripUpdate", entity.Id);
                    continue;
                }

                var tripUpdate = new TripUpdate
                {
                    TripId = entity.TripUpdate.Trip?.TripId ?? string.Empty,
                    RouteId = entity.TripUpdate.Trip?.RouteId ?? string.Empty,
                    ScheduleRelationship = MapScheduleRelationship(entity.TripUpdate.Trip?.ScheduleRelationship),
                    StopTimeUpdates = new List<StopTimeUpdate>()
                };

                _logger.LogDebug("Mapped trip {TripId} with {StopCount} stops",
                    tripUpdate.TripId,
                    entity.TripUpdate.StopTimeUpdate?.Count ?? 0);

                // Map stop time updates
                if (entity.TripUpdate.StopTimeUpdate != null)
                {
                    foreach (var stopUpdate in entity.TripUpdate.StopTimeUpdate)
                    {
                        var stopTimeUpdate = new StopTimeUpdate
                        {
                            StopId = stopUpdate.StopId ?? string.Empty,
                            StopSequence = stopUpdate.StopSequence ?? 0,
                            ArrivalTime = stopUpdate.Arrival?.Time,
                            DepartureTime = stopUpdate.Departure?.Time,
                            Delay = null,
                            ScheduleRelationship = MapScheduleRelationship(stopUpdate.ScheduleRelationship)
                        };
                        tripUpdate.StopTimeUpdates.Add(stopTimeUpdate);
                    }
                }

                response.TripUpdates.Add(tripUpdate);
            }

            _logger.LogInformation("MapToTripUpdateResponse: Result has {Count} trip updates", response.TripUpdates.Count);
            return response;
        }

        private VehiclePositionResponse MapToVehiclePositionResponse(MbtaVehiclePositionResponse? mbtaData)
        {
            var response = new VehiclePositionResponse
            {
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                Vehicles = new List<VehiclePosition>()
            };

            if (mbtaData?.Entity == null) return response;

            foreach (var entity in mbtaData.Entity)
            {
                if (entity.Vehicle == null) continue;

                var vehicle = new VehiclePosition
                {
                    TripId = entity.Vehicle.Trip?.TripId ?? string.Empty,
                    RouteId = entity.Vehicle.Trip?.RouteId ?? string.Empty,
                    Latitude = entity.Vehicle.Position?.Latitude ?? 0,
                    Longitude = entity.Vehicle.Position?.Longitude ?? 0,
                    StopId = entity.Vehicle.StopId,
                    CurrentStopSequence = entity.Vehicle.CurrentStopSequence,
                    CurrentStatus = MapVehicleStatus(entity.Vehicle.CurrentStatus),
                    Timestamp = entity.Vehicle.Timestamp ?? DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    VehicleLabel = entity.Vehicle.Vehicle?.Label,
                    OccupancyStatus = MapOccupancyStatus(entity.Vehicle.OccupancyStatus),
                    Bearing = entity.Vehicle.Position?.Bearing
                };

                response.Vehicles.Add(vehicle);
            }

            return response;
        }

        private ServiceAlertResponse MapToServiceAlertResponse(MbtaServiceAlertResponse? mbtaData)
        {
            var response = new ServiceAlertResponse
            {
                Alerts = new List<ServiceAlert>()
            };

            if (mbtaData?.Entity == null) return response;

            foreach (var entity in mbtaData.Entity)
            {
                if (entity.Alert == null) continue;

                var alert = new ServiceAlert
                {
                    Id = entity.Id ?? string.Empty,
                    InformedEntities = new List<InformedEntity>(),
                    Cause = MapAlertCause(entity.Alert.Cause),
                    Effect = MapAlertEffect(entity.Alert.Effect),
                    HeaderText = entity.Alert.HeaderText?.Translation?.FirstOrDefault()?.Text,
                    DescriptionText = entity.Alert.DescriptionText?.Translation?.FirstOrDefault()?.Text,
                    ServiceEffectText = entity.Alert.ServiceEffectText?.Translation?.FirstOrDefault()?.Text,  // ← Add
                    Url = entity.Alert.Url?.Translation?.FirstOrDefault()?.Text,
                    ImageUrl = entity.Alert.Image?.LocalizedImage?.FirstOrDefault()?.Url,  // ← Add
                    ImageAlternativeText = entity.Alert.ImageAlternativeText?.Translation?.FirstOrDefault()?.Text,  // ← Add
                    TimeframeText = entity.Alert.TimeframeText?.Translation?.FirstOrDefault()?.Text,  // ← Add
                    RecurrenceText = entity.Alert.RecurrenceText?.Translation?.FirstOrDefault()?.Text,  // ← Add
                    StartTime = entity.Alert.ActivePeriod?.FirstOrDefault()?.Start,
                    EndTime = entity.Alert.ActivePeriod?.FirstOrDefault()?.End,
                    SeverityLevel = MapSeverityLevel(entity.Alert.SeverityLevel),
                    CreatedTimestamp = entity.Alert.CreatedTimestamp,  // ← Add
                    LastModifiedTimestamp = entity.Alert.LastModifiedTimestamp
                };

                // Map informed entities
                if (entity.Alert.InformedEntity != null)
                {
                    foreach (var informed in entity.Alert.InformedEntity)
                    {
                        alert.InformedEntities.Add(new InformedEntity
                        {
                            RouteId = informed.RouteId,
                            StopId = informed.StopId,
                            TripId = informed.TripId,
                            AgencyId = informed.AgencyId
                        });
                    }
                }

                response.Alerts.Add(alert);
            }

            return response;
        }

        #endregion
        #region Enum Mappers

        private ScheduleRelationship MapScheduleRelationship(string? relationship)
        {
            return relationship?.ToUpper() switch
            {
                "CANCELED" => ScheduleRelationship.Canceled,
                "SKIPPED" => ScheduleRelationship.Skipped,
                "NO_DATA" => ScheduleRelationship.NoData,
                "ADDED" => ScheduleRelationship.Scheduled, // Added trips treated as scheduled
                _ => ScheduleRelationship.Scheduled
            };
        }

        private VehicleStatus MapVehicleStatus(string? status)
        {
            return status?.ToUpper() switch
            {
                "INCOMING_AT" => VehicleStatus.IncomingAt,
                "STOPPED_AT" => VehicleStatus.StoppedAt,
                "IN_TRANSIT_TO" => VehicleStatus.InTransitTo,
                _ => VehicleStatus.InTransitTo
            };
        }

        private OccupancyStatus? MapOccupancyStatus(string? status)
        {
            return status?.ToUpper() switch
            {
                "EMPTY" => OccupancyStatus.Empty,
                "MANY_SEATS_AVAILABLE" => OccupancyStatus.ManySeatsAvailable,
                "FEW_SEATS_AVAILABLE" => OccupancyStatus.FewSeatsAvailable,
                "STANDING_ROOM_ONLY" => OccupancyStatus.StandingRoomOnly,
                "CRUSHED_STANDING_ROOM_ONLY" => OccupancyStatus.CrushedStandingRoomOnly,
                "FULL" => OccupancyStatus.Full,
                "NOT_ACCEPTING_PASSENGERS" => OccupancyStatus.NotAcceptingPassengers,
                _ => null
            };
        }

        private AlertCause MapAlertCause(string? cause)
        {
            return cause?.ToUpper() switch
            {
                "ACCIDENT" => AlertCause.Accident,
                "CONSTRUCTION" => AlertCause.Construction,
                "MAINTENANCE" => AlertCause.Maintenance,
                "WEATHER" => AlertCause.Weather,
                "TECHNICAL_PROBLEM" => AlertCause.TechnicalProblem,
                "POLICE_ACTIVITY" => AlertCause.PoliceActivity,
                "STRIKE" => AlertCause.Strike,
                "MEDICAL_EMERGENCY" => AlertCause.MedicalEmergency,
                "DEMONSTRATION" => AlertCause.Demonstration,
                "HOLIDAY" => AlertCause.Holiday,
                _ => AlertCause.Unknown
            };
        }

        private AlertEffect MapAlertEffect(string? effect)
        {
            return effect?.ToUpper() switch
            {
                "NO_SERVICE" => AlertEffect.NoService,
                "REDUCED_SERVICE" => AlertEffect.ReducedService,
                "SIGNIFICANT_DELAYS" => AlertEffect.SignificantDelays,
                "DETOUR" => AlertEffect.Detour,
                "ADDITIONAL_SERVICE" => AlertEffect.AdditionalService,
                "MODIFIED_SERVICE" => AlertEffect.ModifiedService,
                "STOP_MOVED" => AlertEffect.StopMoved,
                "ACCESSIBILITY_ISSUE" => AlertEffect.AccessibilityIssue,
                _ => AlertEffect.UnknownEffect
            };
        }

        private int? MapSeverityLevel(string? severityLevel)
        {
            return severityLevel?.ToUpper() switch
            {
                "INFO" => 1,
                "WARNING" => 2,
                "SEVERE" => 3,
                _ => 1
            };
        }

        #endregion
    }

    #region MBTA JSON Models (Internal)

    // TripUpdate Models
    // TripUpdate Models
    internal class MbtaTripUpdateResponse
    {
        [JsonPropertyName("header")]
        public MbtaHeader? Header { get; set; }

        [JsonPropertyName("entity")]
        public List<MbtaTripUpdateEntity>? Entity { get; set; }
    }

    internal class MbtaHeader
    {
        [JsonPropertyName("timestamp")]
        public long Timestamp { get; set; }

        [JsonPropertyName("gtfs_realtime_version")]
        public string? GtfsRealtimeVersion { get; set; }

        [JsonPropertyName("incrementality")]
        public string? Incrementality { get; set; }
    }

    internal class MbtaTripUpdateEntity
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("trip_update")]  // ← snake_case!
        public MbtaTripUpdateData? TripUpdate { get; set; }
    }

    internal class MbtaTripUpdateData
    {
        [JsonPropertyName("timestamp")]
        public long Timestamp { get; set; }

        [JsonPropertyName("trip")]
        public MbtaTripDescriptor? Trip { get; set; }

        [JsonPropertyName("stop_time_update")]  // ← snake_case!
        public List<MbtaStopTimeUpdate>? StopTimeUpdate { get; set; }

        [JsonPropertyName("vehicle")]
        public MbtaVehicleDescriptor? Vehicle { get; set; }
    }

    internal class MbtaTripDescriptor
    {
        [JsonPropertyName("trip_id")]
        public string? TripId { get; set; }

        [JsonPropertyName("route_id")]
        public string? RouteId { get; set; }

        [JsonPropertyName("start_time")]
        public string? StartTime { get; set; }

        [JsonPropertyName("start_date")]
        public string? StartDate { get; set; }

        [JsonPropertyName("direction_id")]
        public int? DirectionId { get; set; }

        [JsonPropertyName("schedule_relationship")]
        public string? ScheduleRelationship { get; set; }

        [JsonPropertyName("revenue")]
        public bool? Revenue { get; set; }
    }

    internal class MbtaStopTimeUpdate
    {
        [JsonPropertyName("stop_id")]
        public string? StopId { get; set; }

        [JsonPropertyName("stop_sequence")]
        public int? StopSequence { get; set; }

        [JsonPropertyName("arrival")]
        public MbtaStopTimeEvent? Arrival { get; set; }

        [JsonPropertyName("departure")]
        public MbtaStopTimeEvent? Departure { get; set; }

        [JsonPropertyName("schedule_relationship")]
        public string? ScheduleRelationship { get; set; }
    }

    internal class MbtaStopTimeEvent
    {
        [JsonPropertyName("time")]
        public long? Time { get; set; }

        [JsonPropertyName("uncertainty")]
        public int? Uncertainty { get; set; }
    }

    internal class MbtaVehicleDescriptor
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("label")]
        public string? Label { get; set; }
    }

    // VehiclePosition Models
    internal class MbtaVehiclePositionResponse
    {
        [JsonPropertyName("entity")]
        public List<MbtaVehiclePositionEntity>? Entity { get; set; }
    }

    internal class MbtaVehiclePositionEntity
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("vehicle")]
        public MbtaVehiclePositionData? Vehicle { get; set; }
    }

    internal class MbtaVehiclePositionData
    {
        [JsonPropertyName("trip")]
        public MbtaTripDescriptor? Trip { get; set; }

        [JsonPropertyName("position")]
        public MbtaPosition? Position { get; set; }

        [JsonPropertyName("current_stop_sequence")]
        public int? CurrentStopSequence { get; set; }

        [JsonPropertyName("stop_id")]
        public string? StopId { get; set; }

        [JsonPropertyName("current_status")]
        public string? CurrentStatus { get; set; }

        [JsonPropertyName("timestamp")]
        public long? Timestamp { get; set; }

        [JsonPropertyName("vehicle")]
        public MbtaVehicleDescriptor? Vehicle { get; set; }

        [JsonPropertyName("occupancy_status")]
        public string? OccupancyStatus { get; set; }
    }

    internal class MbtaPosition
    {
        [JsonPropertyName("latitude")]
        public decimal? Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public decimal? Longitude { get; set; }

        [JsonPropertyName("bearing")]
        public decimal? Bearing { get; set; }

        [JsonPropertyName("speed")]
        public decimal? Speed { get; set; }
    }

    // ServiceAlert Models
    internal class MbtaServiceAlertResponse
    {
        [JsonPropertyName("header")]
        public MbtaHeader? Header { get; set; }

        [JsonPropertyName("entity")]
        public List<MbtaAlertEntity>? Entity { get; set; }
    }

    internal class MbtaAlertEntity
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("alert")]
        public MbtaAlertData? Alert { get; set; }
    }

    internal class MbtaAlertData
    {
        [JsonPropertyName("cause")]
        public string? Cause { get; set; }

        [JsonPropertyName("effect")]
        public string? Effect { get; set; }

        [JsonPropertyName("severity_level")]
        public string? SeverityLevel { get; set; }

        [JsonPropertyName("active_period")]
        public List<MbtaActivePeriod>? ActivePeriod { get; set; }

        [JsonPropertyName("informed_entity")]
        public List<MbtaInformedEntity>? InformedEntity { get; set; }

        [JsonPropertyName("header_text")]
        public MbtaTranslation? HeaderText { get; set; }

        [JsonPropertyName("description_text")]
        public MbtaTranslation? DescriptionText { get; set; }

        [JsonPropertyName("service_effect_text")]
        public MbtaTranslation? ServiceEffectText { get; set; }

        [JsonPropertyName("url")]
        public MbtaUrlTranslation? Url { get; set; }

        [JsonPropertyName("image")]
        public MbtaImage? Image { get; set; }

        [JsonPropertyName("image_alternative_text")]
        public MbtaTranslation? ImageAlternativeText { get; set; }

        [JsonPropertyName("timeframe_text")]
        public MbtaTranslation? TimeframeText { get; set; }

        [JsonPropertyName("recurrence_text")]
        public MbtaTranslation? RecurrenceText { get; set; }

        [JsonPropertyName("created_timestamp")]
        public long? CreatedTimestamp { get; set; }

        [JsonPropertyName("last_modified_timestamp")]
        public long? LastModifiedTimestamp { get; set; }
    }
    internal class MbtaActivePeriod
    {
        [JsonPropertyName("start")]
        public long? Start { get; set; }

        [JsonPropertyName("end")]
        public long? End { get; set; }
    }

    internal class MbtaInformedEntity
    {
        [JsonPropertyName("stop_id")]
        public string? StopId { get; set; }

        [JsonPropertyName("route_id")]
        public string? RouteId { get; set; }

        [JsonPropertyName("trip_id")]
        public string? TripId { get; set; }

        [JsonPropertyName("agency_id")]
        public string? AgencyId { get; set; }

        [JsonPropertyName("direction_id")]
        public int? DirectionId { get; set; }
    }

    internal class MbtaTranslation
    {
        [JsonPropertyName("translation")]
        public List<MbtaTranslationText>? Translation { get; set; }
    }

    internal class MbtaTranslationText
    {
        [JsonPropertyName("text")]
        public string? Text { get; set; }

        [JsonPropertyName("language")]
        public string? Language { get; set; }
    }

    internal class MbtaUrlTranslation
    {
        [JsonPropertyName("translation")]
        public List<MbtaTranslationText>? Translation { get; set; }
    }
   

    internal class MbtaImage
    {
        [JsonPropertyName("localized_image")]
        public List<MbtaLocalizedImage>? LocalizedImage { get; set; }
    }

    internal class MbtaLocalizedImage
    {
        [JsonPropertyName("url")]
        public string? Url { get; set; }

        [JsonPropertyName("media_type")]
        public string? MediaType { get; set; }

        [JsonPropertyName("language")]
        public string? Language { get; set; }
    }

    #endregion
}
