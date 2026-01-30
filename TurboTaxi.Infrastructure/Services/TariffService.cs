using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TurboTaxi.Application.Interfaces;
using TurboTaxi.Domain.Entities;
using TurboTaxi.Infrastructure.Data;

namespace TurboTaxi.Infrastructure.Services
{
    public class TariffService : ITariffService
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<TariffService> _logger;

        public TariffService(ApplicationDbContext db, ILogger<TariffService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<Tariff?> GetTariffByCityKeyAsync(string cityKey, string countryCode = "RU", CancellationToken ct = default)
        {
            return await _db.Tariffs
                .AsNoTracking()
                .FirstOrDefaultAsync(t => 
                    t.CityKey == cityKey && 
                    t.CountryCode == countryCode && 
                    t.IsActive, ct);
        }

        public async Task<Tariff> GetDefaultTariffAsync(string countryCode = "RU", CancellationToken ct = default)
        {
            try
            {
                var defaultTariff = await _db.Tariffs
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => 
                        t.CityKey == $"default_{countryCode.ToLower()}" && 
                        t.CountryCode == countryCode && 
                        t.IsActive, ct);

                if (defaultTariff == null)
                {
                    _logger.LogError("Default tariff for country {CountryCode} not found in database", countryCode);
                    throw new KeyNotFoundException($"Default tariff not configured for country: {countryCode}. Please contact support.");
                }

                return defaultTariff;
            }
            catch (Exception ex) when (ex is not KeyNotFoundException)
            {
                _logger.LogError(ex, "Error retrieving default tariff for country {CountryCode}", countryCode);
                throw new InvalidOperationException("Failed to retrieve pricing information. Please try again.", ex);
            }
        }
    }
}
