using System.ComponentModel.DataAnnotations;

namespace BookingSystem.Dtos.Appointment;

public class RescheduleAppointmentRequest
{
    [Required]
    public Guid ScheduleId { get; set; }

    [Required]
    public Guid NewScheduleId { get; set; }
}
