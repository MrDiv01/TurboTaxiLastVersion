using Microsoft.Extensions.Logging;
using TurboTaxi.Application.Interfaces;
using TurboTaxi.Domain.Entities;
using TurboTaxi.Models.Promos;
using TurboTaxi.Models.Rides;

namespace TurboTaxi.Infrastructure.Services
{
    public class RideEstimateService : IRideEstimateService
    {
        private readonly IReverseGeocodingService _geocoding;
        private readonly ICityResolver _cityResolver;
        private readonly ITariffService _tariffService;
        private readonly IPricingService _pricingService;
        private readonly IPromoService _promoService;
        private readonly ILogger<RideEstimateService> _logger;

        public RideEstimateService(
            IReverseGeocodingService geocoding,
            ICityResolver cityResolver,
            ITariffService tariffService,
            IPricingService pricingService,
            IPromoService promoService,
            ILogger<RideEstimateService> logger)
        {
            _geocoding = geocoding;
            _cityResolver = cityResolver;
            _tariffService = tariffService;
            _pricingService = pricingService;
            _promoService = promoService;
            _logger = logger;
        }

        public async Task<RideEstimateResponse> EstimateRideAsync(RideEstimateRequest request, CancellationToken ct = default)
        {
            // Step 1: Reverse geocode pickup location to get city name
            var detectedCityName = await _geocoding.GetCityFromCoordinatesAsync(
                request.PickupLat, 
                request.PickupLng, 
                ct);

            _logger.LogInformation("Detected city from coordinates ({Lat}, {Lng}): {City}", 
                request.PickupLat, request.PickupLng, detectedCityName ?? "null");

            // Step 2: Resolve city name to city key
            var cityKey = _cityResolver.ResolveCityKey(detectedCityName);
            
            _logger.LogInformation("Resolved city '{CityName}' to city key: {CityKey}", 
                detectedCityName ?? "null", cityKey);

            // Step 3: Get tariff from database
            Tariff? tariff = await _tariffService.GetTariffByCityKeyAsync(cityKey, "RU", ct);
            
            if (tariff == null)
            {
                _logger.LogWarning("Tariff not found for city key {CityKey}, using default", cityKey);
                tariff = await _tariffService.GetDefaultTariffAsync("RU", ct);
                cityKey = "default_ru";
            }

            // Step 4: Calculate distance and duration (mock for now - you can integrate with Google Maps Distance Matrix API)
            var (distanceKm, durationMin) = await CalculateDistanceAndDurationAsync(request, ct);

            // Step 5: Calculate price
            var (rawPrice, finalPrice) = _pricingService.CalculatePrice(tariff, distanceKm, durationMin);

            decimal discount = 0;
            bool promoApplied = false;
            string? promoMessage = null;

            if (!string.IsNullOrWhiteSpace(request.PromoCode) && request.UserId.HasValue && request.UserId.Value > 0)
            {
                var promoResult = await _promoService.ApplyAsync(new ApplyPromoRequest
                {
                    Code = request.PromoCode,
                    UserId = request.UserId.Value,
                    Subtotal = finalPrice,
                    Consume = false
                }, ct);

                promoApplied = promoResult.Success;
                promoMessage = promoResult.Message;
                discount = promoResult.Success ? promoResult.Discount : 0;
                finalPrice = promoResult.Success ? promoResult.FinalTotal : finalPrice;
            }

            // Step 6: Build response
            return new RideEstimateResponse
            {
                DetectedCity = tariff.DisplayCityName,
                CityKey = cityKey,
                DistanceKm = distanceKm,
                DurationMin = durationMin,
                Tariff = new TariffInfo
                {
                    BaseFare = tariff.BaseFare,
                    PricePerKm = tariff.PricePerKm,
                    PricePerMinute = tariff.PricePerMinute,
                    MinimumFare = tariff.MinimumFare
                },
                RawPrice = rawPrice,
                FinalPrice = finalPrice,
                Discount = discount,
                PromoApplied = promoApplied,
                PromoCode = request.PromoCode,
                PromoMessage = promoMessage
            };
        }

        private async Task<(double distanceKm, double durationMin)> CalculateDistanceAndDurationAsync(
            RideEstimateRequest request, 
            CancellationToken ct)
        {
            // TODO: Integrate with Google Maps Distance Matrix API or use existing service
            // For now, using Haversine formula for distance estimation
            double distanceKm = CalculateHaversineDistance(
                request.PickupLat, request.PickupLng,
                request.DropoffLat, request.DropoffLng);

            // Rough estimate: average city speed 30 km/h
            double durationMin = (distanceKm / 30.0) * 60.0;

            await Task.CompletedTask; // Remove this when implementing real API call

            return (Math.Round(distanceKm, 2), Math.Round(durationMin, 1));
        }

        private static double CalculateHaversineDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371; // Earth radius in kilometers

            var lat1Rad = DegreesToRadians(lat1);
            var lat2Rad = DegreesToRadians(lat2);
            var deltaLat = DegreesToRadians(lat2 - lat1);
            var deltaLon = DegreesToRadians(lon2 - lon1);

            var a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                    Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                    Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return R * c;
        }

        private static double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }
    }
}
