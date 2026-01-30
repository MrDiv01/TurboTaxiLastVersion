using System.ComponentModel.DataAnnotations;

namespace TurboTaxi.Models.Auth
{
    public record RefreshTokenRequest
    {
        [Required]
        public string RefreshToken { get; init; } = null!;
    }

    public record RefreshTokenResponse
    {
        public bool Success { get; init; }
        public string? Message { get; init; }
        public TokensDto? Tokens { get; init; }
    }

    public record TokensDto
    {
        public string AccessToken { get; init; } = null!;
        public string RefreshToken { get; init; } = null!;
        public int ExpiresIn { get; init; }
        public string TokenType { get; init; } = "Bearer";
    }

    public record LogoutResponse
    {
        public bool Success { get; init; }
        public string Message { get; init; } = null!;
    }

    public record CurrentUserResponse
    {
        public int Id { get; init; }
        public string FullName { get; init; } = null!;
        public string Email { get; init; } = null!;
        public string? PhoneNumber { get; init; }
        public string Role { get; init; } = "User";
        public int? DriverId { get; init; }
        public DateTime CreatedAt { get; init; }
    }
}
