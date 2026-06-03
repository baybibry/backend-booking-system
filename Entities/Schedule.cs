namespace BookingSystem.Entities;

public class Schedule : BaseDateTime
{
    public Guid ScheduleId { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public bool IsBooked { get; set; }
    public bool IsBlocked { get; set; }

    public Guid DoctorId { get; set; }

    public Doctor Doctor { get; set; } = null!;
    public ICollection<Appointment> Appointments { get; set; } = [];
}
