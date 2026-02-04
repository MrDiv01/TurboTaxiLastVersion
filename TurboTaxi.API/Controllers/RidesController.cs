using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TurboTaxi.Application.Interfaces;
using TurboTaxi.Models.Rides;

namespace TurboTaxi.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RidesController : ControllerBase
    {
        private readonly IRideService _rideService;
        private readonly IRideEstimateService _estimateService;
        private readonly ILogger<RidesController> _logger;

        public RidesController(
            IRideService rideService, 
            IRideEstimateService estimateService,
            ILogger<RidesController> logger)
        {
            _rideService = rideService;
            _estimateService = estimateService;
            _logger = logger;
        }

        /// <summary>
        /// Estimate ride price based on pickup and dropoff coordinates
        /// </summary>
        [HttpPost("estimate")]
        public async Task<ActionResult<RideEstimateResponse>> EstimateRide([FromBody] RideEstimateRequest request, CancellationToken ct)
        {
            try
            {
                // Validation
                if (request == null)
                {
                    return BadRequest(new { success = false, error = "Request body is required" });
                }

                // Populate userId from token if available (promo hesablaması üçün)
                var userIdClaim = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrWhiteSpace(userIdClaim) == false && request.UserId is null)
                {
                    if (int.TryParse(userIdClaim, out var uid))
                    {
                        request.UserId = uid;
                    }
                }

                if (request.PickupLat < -90 || request.PickupLat > 90)
                {
                    return BadRequest(new { success = false, error = "Pickup latitude must be between -90 and 90" });
                }

                if (request.PickupLng < -180 || request.PickupLng > 180)
                {
                    return BadRequest(new { success = false, error = "Pickup longitude must be between -180 and 180" });
                }

                if (request.DropoffLat < -90 || request.DropoffLat > 90)
                {
                    return BadRequest(new { success = false, error = "Dropoff latitude must be between -90 and 90" });
                }

                if (request.DropoffLng < -180 || request.DropoffLng > 180)
                {
                    return BadRequest(new { success = false, error = "Dropoff longitude must be between -180 and 180" });
                }

                _logger.LogInformation("POST /api/rides/estimate - Pickup: ({PickupLat}, {PickupLng}), Dropoff: ({DropoffLat}, {DropoffLng})",
                    request.PickupLat, request.PickupLng, request.DropoffLat, request.DropoffLng);

                var result = await _estimateService.EstimateRideAsync(request, ct);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation during ride estimation");
                return BadRequest(new { success = false, error = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Resource not found during ride estimation");
                return NotFound(new { success = false, error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error estimating ride");
                return StatusCode(500, new { success = false, error = "Failed to estimate ride. Please try again later." });
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<CreateRideResponse>> CreateRide([FromBody] CreateRideRequest request, CancellationToken ct)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new { success = false, error = "Request body is required" });
                }

                // Prefer authenticated user id if token exists; otherwise require body UserId for test mode
                var userIdClaim = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrWhiteSpace(userIdClaim) && int.TryParse(userIdClaim, out var claimUserId) && claimUserId > 0)
                {
                    request.UserId = claimUserId;
                }

                if (request.UserId <= 0)
                {
                    return BadRequest(new { success = false, error = "UserId is required (pass in body or use auth token)" });
                }

                _logger.LogInformation($"🚖 POST /api/rides - Creating ride for User #{request.UserId}");
                var result = await _rideService.CreateRideAsync(request, ct);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Resource not found while creating ride");
                return NotFound(new { success = false, error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while creating ride");
                return BadRequest(new { success = false, error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error creating ride");
                return StatusCode(500, new { success = false, error = "Failed to create ride. Please try again later." });
            }
        }

        [HttpPost("{rideId}/accept")]
        public async Task<ActionResult<RideAcceptedResponse>> AcceptRide(int rideId, [FromBody] AcceptRideRequest request, CancellationToken ct)
        {
            try
            {
                return Ok(await _rideService.AcceptRideAsync(rideId, request, ct));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error accepting ride #{rideId}");
                return StatusCode(500, new { error = "Failed to accept ride", details = ex.Message });
            }
        }

        [HttpPost("{rideId}/arrive")]
        public async Task<ActionResult<RideArrivedResponse>> ArriveRide(int rideId, [FromBody] ArriveRideRequest request, CancellationToken ct)
        {
            try
            {
                return Ok(await _rideService.ArriveRideAsync(rideId, request, ct));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error marking ride #{rideId} as arrived");
                return StatusCode(500, new { error = "Failed to mark ride as arrived", details = ex.Message });
            }
        }

        [HttpPost("{rideId}/start")]
        public async Task<ActionResult<StartRideResponse>> StartRide(int rideId, [FromBody] StartRideRequest request, CancellationToken ct)
        {
            try
            {
                return Ok(await _rideService.StartRideAsync(rideId, request, ct));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error starting ride #{rideId}");
                return StatusCode(500, new { error = "Failed to start ride", details = ex.Message });
            }
        }

        [HttpPost("{rideId}/finish")]
        public async Task<ActionResult<RideFinishedResponse>> FinishRide(int rideId, [FromBody] FinishRideRequest request, CancellationToken ct)
        {
            try
            {
                return Ok(await _rideService.FinishRideAsync(rideId, request, ct));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error finishing ride #{rideId}");
                return StatusCode(500, new { error = "Failed to finish ride", details = ex.Message });
            }
        }

        [HttpPost("{rideId}/cancel-by-user")]
        public async Task<ActionResult<CancelRideResponse>> CancelByUser(int rideId, [FromBody] CancelRideByUserRequest request, CancellationToken ct)
        {
            try
            {
                return Ok(await _rideService.CancelRideByUserAsync(rideId, request, ct));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error canceling ride #{rideId} by user");
                return StatusCode(500, new { error = "Failed to cancel ride", details = ex.Message });
            }
        }

        [HttpPost("{rideId}/cancel-by-driver")]
        public async Task<ActionResult<CancelRideResponse>> CancelByDriver(int rideId, [FromBody] CancelRideByDriverRequest request, CancellationToken ct)
        {
            try
            {
                return Ok(await _rideService.CancelRideByDriverAsync(rideId, request, ct));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error canceling ride #{rideId} by driver");
                return StatusCode(500, new { error = "Failed to cancel ride", details = ex.Message });
            }
        }

        /// <summary>
        /// Get ride details by ID
        /// </summary>
        [HttpGet("{rideId:int}")]
        [Authorize]
        public async Task<ActionResult<RideDetailResponse>> GetById(int rideId, CancellationToken ct)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                var ride = await _rideService.GetByIdAsync(rideId, userId, ct);
                return Ok(ride);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { success = false, message = ex.Message }); }
            catch (UnauthorizedAccessException ex) { return Forbid(); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting ride {RideId}", rideId);
                return StatusCode(500, new { success = false, message = "Failed to get ride" });
            }
        }

        /// <summary>
        /// Get current user's active ride
        /// </summary>
        [HttpGet("active")]
        [Authorize]
        public async Task<ActionResult<ActiveRideResponse>> GetActive(CancellationToken ct)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                var ride = await _rideService.GetActiveRideAsync(userId, ct);
                return Ok(new ActiveRideResponse { Ride = ride });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active ride");
                return StatusCode(500, new { success = false, message = "Failed to get active ride" });
            }
        }

        /// <summary>
        /// Get user's ride history with pagination
        /// </summary>
        [HttpGet("history")]
        [Authorize]
        public async Task<ActionResult<RideHistoryResponse>> GetHistory([FromQuery] RideHistoryQuery query, CancellationToken ct)
        {
            try
            {
                if (query.PageSize > 100) query = query with { PageSize = 100 };

                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                var history = await _rideService.GetHistoryAsync(userId, query, ct);
                return Ok(history);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting ride history");
                return StatusCode(500, new { success = false, message = "Failed to get ride history" });
            }
        }
    }
}
