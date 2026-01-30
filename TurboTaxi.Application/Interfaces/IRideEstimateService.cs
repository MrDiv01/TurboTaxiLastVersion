using TurboTaxi.Models.Rides;

namespace TurboTaxi.Application.Interfaces
{
    public interface IRideEstimateService
    {
        Task<RideEstimateResponse> EstimateRideAsync(RideEstimateRequest request, CancellationToken ct = default);
    }
}
