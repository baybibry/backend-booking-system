using BookingSystem.Dtos.Specialization;

namespace BookingSystem.Dtos.Doctor;

public class DoctorSummaryResponse
{
    public Guid DoctorId { get; set; }
    public string FirstName { get; set; } = null!;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public bool Deactivated { get; set; }
    public SpecializationResponse Specialization { get; set; } = null!;
}
