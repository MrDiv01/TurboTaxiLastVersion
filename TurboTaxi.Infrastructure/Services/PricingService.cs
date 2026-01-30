using TurboTaxi.Application.Interfaces;
using TurboTaxi.Domain.Entities;

namespace TurboTaxi.Infrastructure.Services
{
    public class PricingService : IPricingService
    {
        public (decimal rawPrice, decimal finalPrice) CalculatePrice(Tariff tariff, double distanceKm, double durationMin)
        {
            if (tariff == null)
                throw new ArgumentNullException(nameof(tariff));

            // Calculate raw price: BaseFare + (DistanceKm * PricePerKm) + (DurationMin * PricePerMinute)
            decimal rawPrice = tariff.BaseFare 
                + ((decimal)distanceKm * tariff.PricePerKm) 
                + ((decimal)durationMin * tariff.PricePerMinute);

            // Apply minimum fare
            decimal finalPrice = Math.Max(rawPrice, tariff.MinimumFare);

            // Round to nearest integer (RUB)
            finalPrice = Math.Round(finalPrice, 0, MidpointRounding.AwayFromZero);
            rawPrice = Math.Round(rawPrice, 2, MidpointRounding.AwayFromZero);

            return (rawPrice, finalPrice);
        }
    }
}
