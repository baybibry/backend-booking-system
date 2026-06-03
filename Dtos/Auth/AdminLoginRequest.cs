using BookingSystem.Helper;

namespace BookingSystem.Dtos.Auth;

public class AdminLoginRequest
{
    [RequiredNonEmpty]
    public string Username { get; set; } = null!;

    [RequiredNonEmpty]
    public string Password { get; set; } = null!;
}
