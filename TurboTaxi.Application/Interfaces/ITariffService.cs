using TurboTaxi.Domain.Entities;

namespace TurboTaxi.Application.Interfaces
{
    public interface ITariffService
    {
        Task<Tariff?> GetTariffByCityKeyAsync(string cityKey, string countryCode = "RU", CancellationToken ct = default);
        Task<Tariff> GetDefaultTariffAsync(string countryCode = "RU", CancellationToken ct = default);
    }
}
