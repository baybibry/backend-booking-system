using System.ComponentModel.DataAnnotations;

namespace BookingSystem.Dtos.Appointment;

public class CancelAppointmentRequest
{
    [Required]
    public Guid ScheduleId { get; set; }

    [MaxLength(500)]
    public string? CancellationNote { get; set; }
}
