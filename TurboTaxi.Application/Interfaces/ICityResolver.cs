namespace TurboTaxi.Application.Interfaces
{
    public interface ICityResolver
    {
        string ResolveCityKey(string? cityName);
    }
}
