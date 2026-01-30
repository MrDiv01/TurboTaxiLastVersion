using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace TurboTaxi.Realtime.Hubs
{
    public class UserHub : Hub<IUserClient>
    {
        private readonly ILogger<UserHub> _logger;

        public UserHub(ILogger<UserHub> logger)
        {
            _logger = logger;
        }

        public Task SubscribeUser(string userId)
            => Groups.AddToGroupAsync(Context.ConnectionId, $"user:{userId}");

        public Task UnsubscribeUser(string userId)
            => Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user:{userId}");

        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier; // JWT userId
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"User:{userId}");
                _logger.LogInformation("User {UserId} connected to UserHub. ConnectionId: {ConnectionId}", 
                    userId, Context.ConnectionId);
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.UserIdentifier;
            if (!string.IsNullOrEmpty(userId))
            {
                _logger.LogInformation("User {UserId} disconnected from UserHub. ConnectionId: {ConnectionId}", 
                    userId, Context.ConnectionId);
            }
            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// User aktiv ride group-a subscribe olur
        /// </summary>
        public async Task JoinRide(int rideId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Ride:{rideId}");
            _logger.LogInformation("User {UserId} joined Ride:{RideId} group", 
                Context.UserIdentifier, rideId);
        }

        /// <summary>
        /// User ride group-dan çıxır
        /// </summary>
        public async Task LeaveRide(int rideId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Ride:{rideId}");
            _logger.LogInformation("User {UserId} left Ride:{RideId} group", 
                Context.UserIdentifier, rideId);
        }
    }
}
