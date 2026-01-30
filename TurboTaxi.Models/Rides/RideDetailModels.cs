using TurboTaxi.Domain.Enums;

namespace TurboTaxi.Models.Rides
{
    // Common DTOs
    public record LocationDto(double Lat, double Lng, string? Address);

    public record DriverSummaryDto
    {
        public int Id { get; init; }
        public string Name { get; init; } = null!;
        public string? Phone { get; init; }
        public VehicleDto Vehicle { get; init; } = null!;
        public double? Rating { get; init; }
    }

    public record VehicleDto
    {
        public string Brand { get; init; } = null!;
        public string Model { get; init; } = null!;
        public string Color { get; init; } = null!;
        public string PlateNumber { get; init; } = null!;
        public int Year { get; init; }
    }

    public record UserBasicDto(int Id, string Name, string? Phone);

    public record TariffSnapshotDto
    {
        public string CityName { get; init; } = null!;
        public decimal BaseFare { get; init; }
        public decimal PricePerKm { get; init; }
        public decimal PricePerMinute { get; init; }
        public decimal MinimumFare { get; init; }
    }

    // Ride Detail Response
    public record RideDetailResponse
    {
        public int Id { get; init; }
        public RideStatus Status { get; init; }
        
        // Location
        public LocationDto Pickup { get; init; } = null!;
        public LocationDto Dropoff { get; init; } = null!;
        
        // Pricing
        public decimal EstimatedPrice { get; init; }
        public decimal? FinalPrice { get; init; }
        public TariffSnapshotDto Tariff { get; init; } = null!;
        
        // Participants
        public int UserId { get; init; }
        public string UserName { get; init; } = null!;
        public string? UserPhone { get; init; }
        public int? DriverId { get; init; }
        public DriverSummaryDto? Driver { get; init; }
        
        // Timestamps
        public DateTime CreatedAt { get; init; }
        public DateTime? AcceptedAt { get; init; }
        public DateTime? StartedAt { get; init; }
        public DateTime? FinishedAt { get; init; }
        public DateTime? CanceledAt { get; init; }
        
        // Cancellation
        public string? CanceledBy { get; init; }
        public string? CancellationReason { get; init; }
    }

    // Active Ride
    public record ActiveRideResponse
    {
        public RideDetailResponse? Ride { get; init; }
    }

    // Ride History
    public record RideHistoryQuery
    {
        public int Page { get; init; } = 1;

        public int PageSize { get; init; } = 20;

        public RideStatus? Status { get; init; }

        public DateTime? From { get; init; }

        public DateTime? To { get; init; }
    }

    public record RideHistoryResponse
    {
        public List<RideHistoryItemDto> Rides { get; init; } = new();
        public int TotalCount { get; init; }
        public int Page { get; init; }
        public int PageSize { get; init; }
        public int TotalPages { get; init; }
    }

    public record RideHistoryItemDto
    {
        public int Id { get; init; }
        public RideStatus Status { get; init; }
        public string PickupAddress { get; init; } = null!;
        public string DropoffAddress { get; init; } = null!;
        public decimal Price { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? FinishedAt { get; init; }
        public string? DriverName { get; init; }
    }

    // Cancel Ride Request (Response artıq RideModels.cs-də var)
    public record CancelRideRequest
    {
        public string? Reason { get; init; }
    }
}
