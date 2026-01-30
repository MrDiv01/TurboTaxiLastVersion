using TurboTaxi.Domain.Entities;
using TurboTaxi.Models.Auth;

namespace TurboTaxi.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
        Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);

        // New methods
        Task<RefreshTokenResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken ct = default);
        Task LogoutAsync(int userId, CancellationToken ct = default);
        Task<CurrentUserResponse> GetCurrentUserAsync(int userId, CancellationToken ct = default);
    }
}
