using TurboTaxi.Domain.Enums;
using TurboTaxi.Models.Rides;

namespace TurboTaxi.Models.Realtime
{
    // ===== USER APP EVENTS =====
    
    public record RideCreatedEvent
    {
        public int RideId { get; init; }
        public RideStatus Status { get; init; }
        public int CandidatesCount { get; init; }
        public DateTime CreatedAtUtc { get; init; }
    }

    public record RideAcceptedEvent
    {
        public int RideId { get; init; }
        public RideStatus Status { get; init; }
        public DriverSummaryDto Driver { get; init; } = null!;
        public int? EtaMinutes { get; init; }
        public DateTime AcceptedAtUtc { get; init; }
    }

    public record RideStartedEvent
    {
        public int RideId { get; init; }
        public RideStatus Status { get; init; }
        public DateTime StartedAtUtc { get; init; }
    }

    public record RideFinishedEvent
    {
        public int RideId { get; init; }
        public RideStatus Status { get; init; }
        public decimal FinalPrice { get; init; }
        public DateTime FinishedAtUtc { get; init; }
    }

    public record RideCanceledEvent
    {
        public int RideId { get; init; }
        public RideStatus Status { get; init; }
        public string CanceledBy { get; init; } = null!; // "User" | "Driver" | "System"
        public string? Reason { get; init; }
        public DateTime CanceledAtUtc { get; init; }
    }

    public record DriverLocationEvent
    {
        public int RideId { get; init; }
        public int DriverId { get; init; }
        public double Lat { get; init; }
        public double Lng { get; init; }
        public DateTime UpdatedAtUtc { get; init; }
    }

    // ===== DRIVER APP EVENTS =====
    
    public record RideOfferEvent
    {
        public int RideId { get; init; }
        public LocationDto Pickup { get; init; } = null!;
        public LocationDto Dropoff { get; init; } = null!;
        public decimal EstimatedPrice { get; init; }
        public double DistanceKm { get; init; }
        public UserBasicDto User { get; init; } = null!;
        public DateTime ExpiresAtUtc { get; init; }
    }

    public record RideOfferExpiredEvent
    {
        public int RideId { get; init; }
        public DateTime ExpiredAtUtc { get; init; }
    }

    public record RideStatusChangedEvent
    {
        public int RideId { get; init; }
        public RideStatus OldStatus { get; init; }
        public RideStatus NewStatus { get; init; }
        public DateTime ChangedAtUtc { get; init; }
    }
}
