using BookingSystem.Enums;

namespace BookingSystem.Entities;

public class RefreshToken
{
    public Guid RefreshTokenId { get; set; }
    public string TokenHash { get; set; } = null!;
    public Guid UserId { get; set; }
    public UserRole UserRole { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime CreatedAt { get; set; }
}
