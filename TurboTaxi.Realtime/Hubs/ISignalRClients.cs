using TurboTaxi.Models.Realtime;

namespace TurboTaxi.Realtime.Hubs
{
    /// <summary>
    /// User App üçün SignalR client interface
    /// </summary>
    public interface IUserClient
    {
        Task RideCreated(RideCreatedEvent data);
        Task RideAccepted(RideAcceptedEvent data);
        Task RideStarted(RideStartedEvent data);
        Task RideFinished(RideFinishedEvent data);
        Task RideCanceled(RideCanceledEvent data);
        Task DriverLocationUpdated(DriverLocationEvent data);
        Task RideStatusUpdated(int rideId, string status, object payload);
    }

    // IDriverClient artıq DriverHub.cs-də var, buradan silinir
}
