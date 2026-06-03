using System.ComponentModel.DataAnnotations;

namespace BookingSystem.Dtos.Auth;

public class RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; set; } = null!;
}
