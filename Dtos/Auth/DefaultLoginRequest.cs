using System.ComponentModel.DataAnnotations;
using BookingSystem.Helper;

namespace BookingSystem.Dtos.Auth;

public class DefaultLoginRequest
{
    [RequiredNonEmpty]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [RequiredNonEmpty]
    public string Password { get; set; } = null!;
}
