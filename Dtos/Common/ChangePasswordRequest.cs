using System.ComponentModel.DataAnnotations;
using BookingSystem.Helper;

namespace BookingSystem.Dtos.Common;

public class ChangePasswordRequest
{
    [RequiredNonEmpty]
    public string CurrentPassword { get; set; } = null!;

    [RequiredNonEmpty]
    [MinLength(8, ErrorMessage = "New password must be at least 8 characters.")]
    public string NewPassword { get; set; } = null!;
}
