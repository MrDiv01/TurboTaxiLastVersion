using TurboTaxi.Models.Rides;

namespace TurboTaxi.Application.Interfaces
{
    public interface IRideService
    {
        // Existing methods
        Task<CreateRideResponse> CreateRideAsync(CreateRideRequest request, CancellationToken ct = default);
        Task<RideAcceptedResponse> AcceptRideAsync(int rideId, AcceptRideRequest request, CancellationToken ct = default);
        Task<RideArrivedResponse> ArriveRideAsync(int rideId, ArriveRideRequest request, CancellationToken ct = default);
        Task<StartRideResponse> StartRideAsync(int rideId, StartRideRequest request, CancellationToken ct = default);
        Task<RideFinishedResponse> FinishRideAsync(int rideId, FinishRideRequest request, CancellationToken ct = default);
        Task<CancelRideResponse> CancelRideByUserAsync(int rideId, CancelRideByUserRequest request, CancellationToken ct = default);
        Task<CancelRideResponse> CancelRideByDriverAsync(int rideId, CancelRideByDriverRequest request, CancellationToken ct = default);

        // New GET methods
        Task<RideDetailResponse> GetByIdAsync(int rideId, int requestingUserId, CancellationToken ct = default);
        Task<RideDetailResponse?> GetActiveRideAsync(int userId, CancellationToken ct = default);
        Task<RideHistoryResponse> GetHistoryAsync(int userId, RideHistoryQuery query, CancellationToken ct = default);
        Task<CancelRideResponse> CancelRideAsync(int rideId, int userId, string? reason, string canceledBy, CancellationToken ct = default);
    }
}
