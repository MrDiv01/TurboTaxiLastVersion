using Microsoft.EntityFrameworkCore;
using TurboTaxi.Application.Interfaces;
using TurboTaxi.Domain.Entities;
using TurboTaxi.Infrastructure.Data;
using TurboTaxi.Models.Auth;

namespace TurboTaxi.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _db;
        private readonly IJWTService _jwt;
        public AuthService(ApplicationDbContext db, IJWTService jwt)
        {
            _db = db;
            _jwt = jwt;
        }
        public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
        {
            var exists = await _db.Users.AnyAsync(u => u.Email == request.Email, ct);
            if (exists)
            {
                throw new InvalidOperationException("Email already registered");
            }

            var user = new User
            {
                FullName = request.FullName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = request.Role ?? "User"
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync(ct);

            if (user.Role == "Driver")
            {
                var driver = new Driver
                {
                    UserId = user.Id,
                    IsVerified = false
                };
                _db.Drivers.Add(driver);
                await _db.SaveChangesAsync(ct);
            }

            return new AuthResponse
            {
                Success = true,
                Message = "Registration successful",
                Data = new AuthResponseData
                {
                    User = new UserDto
                    {
                        Id = user.Id,
                        FullName = user.FullName,
                        PhoneNumber = user.PhoneNumber,
                        Email = user.Email,
                        Role = user.Role,
                        CreatedAt = user.CreatedTime
                    },
                    Tokens = null!
                }
            };
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email, ct);

            if (user is null)
            {
                throw new KeyNotFoundException("User with this email not found");
            }

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Invalid password");
            }

            return await BuildAuthResponseAsync(user, "Login successful", ct);
        }

        private async Task<AuthResponse> BuildAuthResponseAsync(User user, string message, CancellationToken ct)
        {
            var accessToken = _jwt.GenerateJwtToken(user);
            var refreshToken = Guid.NewGuid().ToString("N");

            // Save refresh token to DB
            _db.RefreshTokens.Add(new RefreshToken
            {
                UserId = user.Id,
                Token = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(30),
                IsRevoked = false
            });

            await _db.SaveChangesAsync(ct);

            var response = new AuthResponse
            {
                Success = true,
                Message = message,
                Data = new AuthResponseData
                {
                    User = new UserDto
                    {
                        Id = user.Id,
                        FullName = user.FullName,
                        PhoneNumber = user.PhoneNumber,
                        Email = user.Email,
                        Role = user.Role,
                        CreatedAt = user.CreatedTime
                    },
                    Tokens = new AuthTokensDto
                    {
                        AccessToken = accessToken,
                        RefreshToken = refreshToken,
                        ExpiresIn = 3600,
                        TokenType = "Bearer"
                    }
                }
            };
            return response;
        }

        public async Task<RefreshTokenResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken ct = default)
        {
            // Validate refresh token from DB
            var refreshToken = await _db.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken && !rt.IsRevoked, ct);

            if (refreshToken == null)
            {
                throw new UnauthorizedAccessException("Invalid refresh token");
            }

            if (refreshToken.ExpiresAt < DateTime.UtcNow)
            {
                throw new UnauthorizedAccessException("Refresh token expired");
            }

            // Generate new tokens
            var newAccessToken = _jwt.GenerateJwtToken(refreshToken.User);
            var newRefreshToken = Guid.NewGuid().ToString("N");

            // Revoke old refresh token
            refreshToken.IsRevoked = true;
            refreshToken.RevokedAt = DateTime.UtcNow;

            // Save new refresh token
            _db.RefreshTokens.Add(new RefreshToken
            {
                UserId = refreshToken.UserId,
                Token = newRefreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(30),
                IsRevoked = false
            });

            await _db.SaveChangesAsync(ct);

            return new RefreshTokenResponse
            {
                Success = true,
                Message = "Token refreshed successfully",
                Tokens = new TokensDto
                {
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshToken,
                    ExpiresIn = 3600,
                    TokenType = "Bearer"
                }
            };
        }

        public async Task LogoutAsync(int userId, CancellationToken ct = default)
        {
            // Revoke all active refresh tokens for this user
            var tokens = await _db.RefreshTokens
                .Where(rt => rt.UserId == userId && !rt.IsRevoked)
                .ToListAsync(ct);

            foreach (var token in tokens)
            {
                token.IsRevoked = true;
                token.RevokedAt = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync(ct);
        }

        public async Task<CurrentUserResponse> GetCurrentUserAsync(int userId, CancellationToken ct = default)
        {
            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.Id == userId, ct);

            if (user == null)
            {
                throw new KeyNotFoundException($"User {userId} not found");
            }

            // Check if user is a driver
            var driver = await _db.Drivers
                .FirstOrDefaultAsync(d => d.UserId == userId, ct);

            return new CurrentUserResponse
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Role = driver != null ? "Driver" : "User",
                DriverId = driver?.Id,
                CreatedAt = user.CreatedTime
            };
        }
    }
}
