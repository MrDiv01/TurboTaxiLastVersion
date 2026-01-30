namespace TurboTaxi.Models.Realtime
{
    public class DriverLocationUpdateRequest
    {
        public int DriverId { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public string VehicleType { get; set; } = "Standard";

        public string? DriverStatus { get; set; }
    }
}
