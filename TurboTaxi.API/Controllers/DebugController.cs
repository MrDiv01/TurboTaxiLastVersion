using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TurboTaxi.Infrastructure.Data;

namespace TurboTaxi.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DebugController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<DebugController> _logger;

        public DebugController(ApplicationDbContext db, ILogger<DebugController> logger)
        {
            _db = db;
            _logger = logger;
        }

        /// <summary>
        /// Check if tariffs exist in database
        /// </summary>
        [HttpGet("tariffs")]
        public async Task<IActionResult> GetTariffs()
        {
            var tariffs = await _db.Tariffs
                .Where(t => t.CountryCode == "RU")
                .Select(t => new
                {
                    t.Id,
                    t.CityKey,
                    t.DisplayCityName,
                    t.BaseFare,
                    t.MinimumFare,
                    t.IsActive
                })
                .ToListAsync();

            return Ok(new
            {
                count = tariffs.Count,
                tariffs = tariffs
            });
        }

        /// <summary>
        /// Test tariff lookup by city key
        /// </summary>
        [HttpGet("tariff/{cityKey}")]
        public async Task<IActionResult> GetTariffByCityKey(string cityKey)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(cityKey))
                {
                    return BadRequest(new { success = false, error = "City key is required" });
                }

                var tariff = await _db.Tariffs
                    .FirstOrDefaultAsync(t => t.CityKey == cityKey && t.CountryCode == "RU");

                if (tariff == null)
                {
                    return NotFound(new { success = false, error = $"Tariff not found for city: {cityKey}" });
                }

                return Ok(new
                {
                    success = true,
                    tariff = new
                    {
                        tariff.Id,
                        tariff.CityKey,
                        tariff.DisplayCityName,
                        tariff.BaseFare,
                        tariff.PricePerKm,
                        tariff.PricePerMinute,
                        tariff.MinimumFare
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tariff by city key");
                return StatusCode(500, new { success = false, error = "Failed to retrieve tariff" });
            }
        }

        /// <summary>
        /// Test database connection
        /// </summary>
        [HttpGet("db-connection")]
        public async Task<IActionResult> TestDatabaseConnection()
        {
            try
            {
                var canConnect = await _db.Database.CanConnectAsync();
                
                if (!canConnect)
                {
                    return StatusCode(500, new { success = false, message = "Cannot connect to database" });
                }

                var tariffCount = await _db.Tariffs.CountAsync();

                return Ok(new
                {
                    success = true,
                    message = "Database connection successful",
                    tariffCount = tariffCount
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Database connection failed",
                    error = ex.Message
                });
            }
        }
    }
}
