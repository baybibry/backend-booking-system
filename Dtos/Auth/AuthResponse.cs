using BookingSystem.Enums;

namespace BookingSystem.Dtos.Auth;

public class AuthResponse
{
    public string Token { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
    public UserRole Role { get; set; }
}
