namespace TurboTaxi.Application.Interfaces
{
    public interface IDriverStatusService
    {
        Task UpdateDriverStatusAsync(int driverId, bool isAvailable, double latitude, double longitude, CancellationToken ct = default);
    }
}
