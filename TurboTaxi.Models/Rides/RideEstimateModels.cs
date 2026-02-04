namespace TurboTaxi.Models.Rides
{
    public class RideEstimateRequest
    {
        public double PickupLat { get; set; }
        public double PickupLng { get; set; }
        public double DropoffLat { get; set; }
        public double DropoffLng { get; set; }
        public string? VehicleType { get; set; } = "Standard";

        public string? PromoCode { get; set; }
        public int? UserId { get; set; }
    }

    public class RideEstimateResponse
    {
        public bool Success { get; set; } = true;
        public string DetectedCity { get; set; } = null!;
        public string CityKey { get; set; } = null!;
        public double DistanceKm { get; set; }
        public double DurationMin { get; set; }
        public TariffInfo Tariff { get; set; } = null!;
        public decimal RawPrice { get; set; }
        public decimal FinalPrice { get; set; }

        public decimal Discount { get; set; }
        public string? PromoCode { get; set; }
        public bool PromoApplied { get; set; }
        public string? PromoMessage { get; set; }
    }

    public class TariffInfo
    {
        public decimal BaseFare { get; set; }
        public decimal PricePerKm { get; set; }
        public decimal PricePerMinute { get; set; }
        public decimal MinimumFare { get; set; }
    }
}
