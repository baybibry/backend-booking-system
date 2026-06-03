using BookingSystem.Enums;

namespace BookingSystem.Entities;

public class Appointment : BaseDateTime
{
    public Guid AppointmentId { get; set; }
    public AppointmentStatus Status { get; set; }
    public string? Reason { get; set; }
    public string? Notes { get; set; }
    public string? Diagnosis { get; set; }
    public bool AutoCompleted { get; set; }
    public string? CancellationNote { get; set; }
    public CancelledBy? CancelledBy { get; set; }

    public Guid PatientId { get; set; }
    public Guid DoctorId { get; set; }
    public Guid ScheduleId { get; set; }

    public Guid? ReceptionistId { get; set; }

    public Patient Patient { get; set; } = null!;
    public Doctor Doctor { get; set; } = null!;
    public Schedule Schedule { get; set; } = null!;
    public Receptionist? Receptionist { get; set; }
}
