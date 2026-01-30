namespace TurboTaxi.Application.Interfaces
{
    public interface IReverseGeocodingService
    {
        Task<string?> GetCityFromCoordinatesAsync(double latitude, double longitude, CancellationToken ct = default);
    }
}
