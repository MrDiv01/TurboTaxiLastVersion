using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TurboTaxi.Application.CQRS.Auth.Commands.Login;
using TurboTaxi.Application.CQRS.Auth.Commands.Register;
using TurboTaxi.Application.Interfaces;
using TurboTaxi.Models.Auth;

namespace TurboTaxi.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<AuthController> _logger;
        private readonly IAuthService _authService;

        public AuthController(IMediator mediator, ILogger<AuthController> logger, IAuthService authService)
        {
            _mediator = mediator;
            _logger = logger;
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new { success = false, message = "Request body is required" });
                }

                if (string.IsNullOrWhiteSpace(request.Email))
                {
                    return BadRequest(new { success = false, message = "Email is required" });
                }

                if (string.IsNullOrWhiteSpace(request.Password))
                {
                    return BadRequest(new { success = false, message = "Password is required" });
                }

                if (string.IsNullOrWhiteSpace(request.FullName))
                {
                    return BadRequest(new { success = false, message = "Full name is required" });
                }

                var result = await _mediator.Send(RegisterCommand.FromDto(request));
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Registration validation failed");
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user registration");
                return StatusCode(500, new { success = false, message = "Registration failed. Please try again later." });
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new { success = false, message = "Request body is required" });
                }

                if (string.IsNullOrWhiteSpace(request.Email))
                {
                    return BadRequest(new { success = false, message = "Email is required" });
                }

                if (string.IsNullOrWhiteSpace(request.Password))
                {
                    return BadRequest(new { success = false, message = "Password is required" });
                }

                var result = await _mediator.Send(LoginCommand.FromDto(request));
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Login failed: user not found - {Email}", request.Email);
                return Unauthorized(new { success = false, message = "Invalid email or password" });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Login failed: invalid password - {Email}", request.Email);
                return Unauthorized(new { success = false, message = "Invalid email or password" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user login");
                return StatusCode(500, new { success = false, message = "Login failed. Please try again later." });
            }
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<ActionResult<RefreshTokenResponse>> Refresh([FromBody] RefreshTokenRequest request, CancellationToken ct)
        {
            try
            {
                var result = await _authService.RefreshTokenAsync(request, ct);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { success = false, message = ex.Message });
            }
            catch { return StatusCode(500, new { success = false, message = "Token refresh failed" }); }
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<ActionResult<LogoutResponse>> Logout(CancellationToken ct)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                await _authService.LogoutAsync(userId, ct);
                return Ok(new LogoutResponse { Success = true, Message = "Logged out successfully" });
            }
            catch { return StatusCode(500, new { success = false, message = "Logout failed" }); }
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<CurrentUserResponse>> Me(CancellationToken ct)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                var user = await _authService.GetCurrentUserAsync(userId, ct);
                return Ok(user);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { success = false, message = ex.Message }); }
            catch { return StatusCode(500, new { success = false, message = "Failed to get user info" }); }
        }
    }
}
