using System.ComponentModel.DataAnnotations;
using BookingSystem.Helper;

namespace BookingSystem.Dtos.Appointment;

public class BookAppointmentRequest
{
    [Required]
    public Guid ScheduleId { get; set; }

    [MaxLength(500)]
    public string? Reason { get; set; }
}
