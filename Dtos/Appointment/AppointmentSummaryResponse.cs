using BookingSystem.Dtos.Doctor;
using BookingSystem.Dtos.Patient;
using BookingSystem.Dtos.Schedule;
using BookingSystem.Enums;

namespace BookingSystem.Dtos.Appointment;

public class AppointmentSummaryResponse
{
    public Guid AppointmentId { get; set; }
    public AppointmentStatus Status { get; set; }
    public string? Reason { get; set; }
    public CancelledBy? CancelledBy { get; set; }
    public string? CancellationNote { get; set; }
    public DateTime CreatedAt { get; set; }
    public PatientSummaryResponse? Patient { get; set; }
    public DoctorSummaryResponse Doctor { get; set; } = null!;
    public ScheduleResponse Schedule { get; set; } = null!;
}
