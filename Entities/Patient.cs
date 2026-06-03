namespace BookingSystem.Entities;

public class Patient : BasePerson
{
    public Guid PatientId { get; set; }
    public string? Address { get; set; }

    public ICollection<Appointment> Appointments { get; set; } = [];
}
