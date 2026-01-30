using TurboTaxi.Domain.Enums;
using TurboTaxi.Models.Rides;
using System.Text.Json.Serialization;

namespace TurboTaxi.Models.Drivers
{
    // Update Status Request (for UI toggle)
    public record UpdateDriverStatusRequest
    {
        [JsonPropertyName("isAvailable")]
        public bool IsAvailable { get; init; }

        [JsonPropertyName("latitude")]
        public double Latitude { get; init; }

        [JsonPropertyName("longitude")]
        public double Longitude { get; init; }
    }

    // Go Online/Offline
    public record GoOnlineRequest
    {
        public double Latitude { get; init; }
        public double Longitude { get; init; }
        public string VehicleType { get; init; } = "Standard";
    }

    public record GoOnlineResponse
    {
        public bool Success { get; init; }
        public string Status { get; init; } = "Online";
        public string Message { get; init; } = "You are now online";
    }

    public record GoOfflineResponse
    {
        public bool Success { get; init; }
        public string Status { get; init; } = "Offline";
        public string Message { get; init; } = "You are now offline";
    }

    // Driver Status
    public record DriverStatusResponse
    {
        public int DriverId { get; init; }
        public string Status { get; init; } // Online, Offline, Busy
        public ActiveRideDto? ActiveRide { get; init; }
        public DateTime? LastLocationUpdate { get; init; }
        public LocationDto? CurrentLocation { get; init; }
    }

    public record ActiveRideDto
    {
        public int RideId { get; init; }
        public RideStatus Status { get; init; }
        public UserBasicDto User { get; init; } = null!;
        public LocationDto Pickup { get; init; } = null!;
        public LocationDto Dropoff { get; init; } = null!;
        public decimal EstimatedPrice { get; init; }
        public DateTime CreatedAt { get; init; }
    }

    public record DriverActiveRideResponse
    {
        public ActiveRideDto? Ride { get; init; }
    }
}
