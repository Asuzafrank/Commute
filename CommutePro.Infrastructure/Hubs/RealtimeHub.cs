using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace CommutePro.Infrastructure.Hubs
{
    //[Authorize] // Only authenticated users can connect
    public class RealtimeHub : Hub
    {
        private readonly ILogger<RealtimeHub> _logger;

        public RealtimeHub(ILogger<RealtimeHub> logger)
        {
            _logger = logger;
        }

        // Called when a client connects
        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        // Called when a client disconnects
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (exception != null)
            {
                _logger.LogError(exception, "Client disconnected with error: {ConnectionId}", Context.ConnectionId);
            }
            else
            {
                _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
            }
            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Subscribe to a station - client will receive updates for this station
        /// </summary>
        public async Task SubscribeToStation(string stationId)
        {
            //connection to a group (like a chat room for this station)
            await Groups.AddToGroupAsync(Context.ConnectionId, $"station_{stationId}");

            _logger.LogInformation("Client {ConnectionId} subscribed to station {StationId}",
                Context.ConnectionId, stationId);
        }

        /// <summary>
        /// Unsubscribe from a station
        /// </summary>
        public async Task UnsubscribeFromStation(string stationId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"station_{stationId}");

            _logger.LogInformation("Client {ConnectionId} unsubscribed from station {StationId}",
                Context.ConnectionId, stationId);
        }

        /// <summary>
        /// Get current connection ID (for debugging)
        /// </summary>
        public string GetConnectionId()
        {
            return Context.ConnectionId;
        }

    }
}
