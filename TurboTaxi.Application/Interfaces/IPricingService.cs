using TurboTaxi.Domain.Entities;

namespace TurboTaxi.Application.Interfaces
{
    public interface IPricingService
    {
        (decimal rawPrice, decimal finalPrice) CalculatePrice(Tariff tariff, double distanceKm, double durationMin);
    }
}
