using Microsoft.Extensions.Logging;
using TurboTaxi.Application.Interfaces;
using TurboTaxi.Infrastructure.Data;

namespace TurboTaxi.Infrastructure.Services
{
    public class DriverStatusService : IDriverStatusService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DriverStatusService> _logger;

        public DriverStatusService(ApplicationDbContext context, ILogger<DriverStatusService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task UpdateDriverStatusAsync(int driverId, bool isAvailable, double latitude, double longitude, CancellationToken ct = default)
        {
            var driver = await _context.Drivers.FindAsync([driverId], ct);
            if (driver == null)
            {
                throw new KeyNotFoundException($"Driver with ID {driverId} not found");
            }

            driver.IsAvailable = isAvailable;
            driver.CurrentLatitude = latitude;
            driver.CurrentLongitude = longitude;
            driver.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("Driver #{DriverId} DB status updated to {Status} at ({Lat}, {Lng})", 
                driverId, isAvailable ? "Available" : "Offline", latitude, longitude);
        }
    }
}
