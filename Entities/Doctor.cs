namespace BookingSystem.Entities;

public class Doctor : BasePerson
{
    public Guid DoctorId { get; set; }
    public string LicenseNo { get; set; } = null!;

    public Guid SpecializationId { get; set; }

    public Specialization Specialization { get; set; } = null!;
    public ICollection<Schedule> Schedules { get; set; } = [];
    public ICollection<Appointment> Appointments { get; set; } = [];
}
