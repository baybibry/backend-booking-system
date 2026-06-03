using System.ComponentModel.DataAnnotations;
using BookingSystem.Helper;

namespace BookingSystem.Dtos.Patient;

public class PatientRegistrationRequest
{
    [RequiredNonEmpty]
    [MaxLength(100)]
    public string FirstName { get; set; } = null!;

    [MaxLength(100)]
    public string? MiddleName { get; set; }

    [RequiredNonEmpty]
    [MaxLength(100)]
    public string LastName { get; set; } = null!;

    [RequiredNonEmpty]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = null!;

    [RequiredNonEmpty]
    [Phone]
    [MaxLength(20)]
    public string Phone { get; set; } = null!;

    [RequiredNonEmpty]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
    public string Password { get; set; } = null!;

    [MaxLength(500)]
    public string? Address { get; set; }
}
