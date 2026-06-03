using System.ComponentModel.DataAnnotations;
using BookingSystem.Helper;

namespace BookingSystem.Dtos.Receptionist;

public class ReceptionistRegistrationRequest
{
    [Required]
    [RequiredNonEmpty]
    [MaxLength(100)]
    public string FirstName { get; set; } = null!;

    [MaxLength(100)]
    public string? MiddleName { get; set; }

    [Required]
    [RequiredNonEmpty]
    [MaxLength(100)]
    public string LastName { get; set; } = null!;

    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = null!;

    [Required]
    [Phone]
    [MaxLength(20)]
    public string Phone { get; set; } = null!;

    [Required]
    [RequiredNonEmpty]
    [MaxLength(100)]
    public string EmployeeNo { get; set; } = null!;

    [Required]
    [MinLength(8)]
    public string Password { get; set; } = null!;
}
