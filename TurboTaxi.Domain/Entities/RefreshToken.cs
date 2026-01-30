using TurboTaxi.Domain.BaseEntities;

namespace TurboTaxi.Domain.Entities
{
    public class RefreshToken : BaseEntity
    {
        public int UserId { get; set; }
        public string Token { get; set; } = null!;
        public string? JwtId { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsRevoked { get; set; }
        public DateTime? RevokedAt { get; set; }
        
        // Navigation
        public User User { get; set; } = null!;
    }
}
