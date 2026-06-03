using System.ComponentModel.DataAnnotations;
using BookingSystem.Helper;

namespace BookingSystem.Dtos.Admin;

public class AdminRegistrationRequest
{
    [RequiredNonEmpty]
    [MaxLength(100)]
    public string Username { get; set; } = null!;

    [RequiredNonEmpty]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
    public string Password { get; set; } = null!;
}
