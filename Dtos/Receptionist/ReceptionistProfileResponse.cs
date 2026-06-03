namespace BookingSystem.Dtos.Receptionist;

public class ReceptionistProfileResponse
{
    public Guid ReceptionistId { get; set; }
    public string FirstName { get; set; } = null!;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string EmployeeNo { get; set; } = null!;
    public bool Deactivated { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
