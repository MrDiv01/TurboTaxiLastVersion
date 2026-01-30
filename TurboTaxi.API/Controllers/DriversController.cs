using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TurboTaxi.Application.CQRS.Drivers.Commands.Create;
using TurboTaxi.Application.CQRS.Drivers.Commands.Delete;
using TurboTaxi.Application.CQRS.Drivers.Commands.Update;
using TurboTaxi.Application.CQRS.Drivers.Queries.GetAll;
using TurboTaxi.Application.CQRS.Drivers.Queries.GetById;
using TurboTaxi.Application.CQRS.Drivers.Queries.GetDetailById;
using TurboTaxi.Application.Interfaces;
using TurboTaxi.Models.Drivers;
using TurboTaxi.Realtime.Redis;

namespace TurboTaxi.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DriversController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IDriverStatusService _statusService;
        private readonly IDriverCrudService _driverService;
        private readonly IDriverLocationService _locationService;
        private readonly IRedisService _redisService;
        private readonly IRideNotificationService _rideNotifications;
        private readonly ILogger<DriversController> _logger;

        public DriversController(
            IMediator mediator,
            IDriverStatusService statusService,
            IDriverCrudService driverService,
            IDriverLocationService locationService,
            IRedisService redisService,
            IRideNotificationService rideNotifications,
            ILogger<DriversController> logger)
        {
            _mediator = mediator;
            _statusService = statusService;
            _driverService = driverService;
            _locationService = locationService;
            _redisService = redisService;
            _rideNotifications = rideNotifications;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<DriverDto>>> GetAll()
            => Ok(await _mediator.Send(new GetAllDriversQuery()));

        /// <summary>
        /// Get driver details by ID - includes user info (name, phone, email) and vehicle data
        /// </summary>
        /// <param name="id">Driver ID</param>
        /// <returns>Driver details with user and vehicle information</returns>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<DriverDetailDto>> GetById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { success = false, message = "Invalid driver ID" });
                }

                var result = await _mediator.Send(new GetDriverDetailQuery(id));
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = $"Driver with ID {id} not found" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Failed to retrieve driver details" });
            }
        }

        [HttpPost]
        public async Task<ActionResult<DriverDto>> Create([FromBody] DriverCreateRequest request)
            => Ok(await _mediator.Send(new CreateDriverCommand(request)));

        [HttpPut("{id:int}")]
        public async Task<ActionResult<DriverDto>> Update(int id, [FromBody] DriverUpdateRequest request)
            => Ok(await _mediator.Send(new UpdateDriverCommand(id, request)));

        [HttpDelete("{id:int}")]
        public async Task<ActionResult<bool>> Delete(int id)
            => Ok(await _mediator.Send(new DeleteDriverCommand(id)));

        // Get current authenticated driver's profile by token
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> Me(CancellationToken ct)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (userId == 0)
                {
                    return Unauthorized(new { success = false, message = "User not authenticated" });
                }

                var driver = await _driverService.GetByUserIdAsync(userId, ct);
                if (driver == null)
                {
                    return NotFound(new { success = false, message = "Driver profile not found for this user" });
                }

                return Ok(driver);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current driver profile");
                return StatusCode(500, new { success = false, message = "Failed to get driver profile" });
            }
        }

        [HttpPost("status")]
        [Authorize]
        public async Task<IActionResult> UpdateStatus([FromBody] UpdateDriverStatusRequest request)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (userId == 0)
                {
                    return Unauthorized(new { success = false, message = "User not authenticated" });
                }

                var driver = await _driverService.GetByUserIdAsync(userId, CancellationToken.None);
                if (driver == null)
                {
                    return BadRequest(new { success = false, message = "Driver profile not found for this user" });
                }

                _logger.LogInformation("Driver #{DriverId} (User #{UserId}) updating status to {Status} at ({Lat}, {Lng})",
                    driver.Id, userId, request.IsAvailable ? "Online" : "Offline", request.Latitude, request.Longitude);

                _logger.LogInformation("Latitude check: IsZero={IsZero}, Value={Value}", request.Latitude == 0, request.Latitude);
                _logger.LogInformation("Longitude check: IsZero={IsZero}, Value={Value}", request.Longitude == 0, request.Longitude);

                // Update DB
                await _statusService.UpdateDriverStatusAsync(driver.Id, request.IsAvailable, request.Latitude, request.Longitude);

                // Update Redis (for nearby driver search)
                if (request.IsAvailable && request.Latitude != 0 && request.Longitude != 0)
                {
                    try
                    {
                        _logger.LogInformation("Attempting to push driver #{DriverId} location to Redis: ({Lat}, {Lng})", 
                            driver.Id, request.Latitude, request.Longitude);

                        await _locationService.UpdateAsync(new TurboTaxi.Models.Realtime.DriverLocationUpdateRequest
                        {
                            DriverId = driver.Id,
                            Latitude = request.Latitude,
                            Longitude = request.Longitude,
                            VehicleType = "Standard",
                            DriverStatus = "Free"
                        });
                        _logger.LogInformation("✅ Driver #{DriverId} location pushed to Redis successfully", driver.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "❌ Failed to push driver #{DriverId} location to Redis: {Error}", driver.Id, ex.Message);
                        _logger.LogError("Stack trace: {StackTrace}", ex.StackTrace);
                    }
                }

                await NotifyActiveUserAboutDriverStatusAsync(driver.Id, request.IsAvailable ? "Online" : "Offline");

                return Ok(new
                {
                    success = true,
                    status = request.IsAvailable ? "Online" : "Offline",
                    message = request.IsAvailable ? "You are now online" : "You are now offline"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating driver status");
                return StatusCode(500, new { success = false, message = "Failed to update status" });
            }
        }

        private async Task NotifyActiveUserAboutDriverStatusAsync(int driverId, string newStatus)
        {
            try
            {
                var hashKey = RedisKeys.DriverHash(driverId);
                var userIdStr = await _redisService.HashGetAsync(hashKey, "currentUserId");
                var rideIdStr = await _redisService.HashGetAsync(hashKey, "currentRideId");

                if (!int.TryParse(userIdStr, out var userId) || userId <= 0)
                {
                    return;
                }

                if (!int.TryParse(rideIdStr, out var rideId))
                {
                    rideId = 0;
                }

                await _rideNotifications.NotifyUserAsync(userId, new
                {
                    RideId = rideId,
                    Status = $"Driver{newStatus}",
                    DriverStatus = newStatus,
                    Message = $"Driver is now {newStatus.ToLower()}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to notify user about driver #{DriverId} status change", driverId);
            }
        }
    }
}
