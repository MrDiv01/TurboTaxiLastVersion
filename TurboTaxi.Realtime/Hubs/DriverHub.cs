using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using TurboTaxi.Models.Realtime;

namespace TurboTaxi.Realtime.Hubs
{
    public interface IDriverClient
    {
        Task DriverLocationUpdated(int driverId, double lat, double lng);
        Task NewRideRequest(string requestId, object payload);
        Task RideStatusUpdated(int rideId, string status, object payload);

        // New standardized events
        Task RideOfferSent(RideOfferEvent data);
        Task RideOfferExpired(RideOfferExpiredEvent data);
        Task RideCanceled(RideCanceledEvent data);
        Task RideStatusChanged(RideStatusChangedEvent data);
    }

    public class DriverHub : Hub<IDriverClient>
    {
        private readonly ILogger<DriverHub> _logger;

        public DriverHub(ILogger<DriverHub> logger)
        {
            _logger = logger;
        }

        public Task SubscribeDriver(string driverId)
            => Groups.AddToGroupAsync(Context.ConnectionId, $"driver:{driverId}");

        public Task UnsubscribeDriver(string driverId)
            => Groups.RemoveFromGroupAsync(Context.ConnectionId, $"driver:{driverId}");

        public Task SubscribeUser(string userId)
            => Groups.AddToGroupAsync(Context.ConnectionId, $"user:{userId}");

        public Task UnsubscribeUser(string userId)
            => Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user:{userId}");

        public async Task UpdateLocation(double latitude, double longitude)
        {
            try
            {
                var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("UpdateLocation called without authenticated user");
                    throw new HubException("User not authenticated");
                }

                _logger.LogInformation("Driver {UserId} location updated: ({Lat}, {Lng})", userId, latitude, longitude);

                // Broadcast to all clients subscribed to this driver
                await Clients.Group($"driver:{userId}").DriverLocationUpdated(int.Parse(userId), latitude, longitude);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating driver location");
                throw new HubException("Failed to update location");
            }
        }
    }
}
