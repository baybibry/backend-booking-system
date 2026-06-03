using System.ComponentModel.DataAnnotations;
using BookingSystem.Helper;

namespace BookingSystem.Dtos.Doctor;

public class DoctorRegistrationRequest
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
    [MaxLength(100)]
    public string LicenseNo { get; set; } = null!;

    [RequiredNonEmpty]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
    public string Password { get; set; } = null!;

    [Required]
    public Guid SpecializationId { get; set; }
}
