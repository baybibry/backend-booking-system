using System.ComponentModel.DataAnnotations;

namespace BookingSystem.Dtos.Appointment;

public class ReceptionistCancelAppointmentRequest
{
    [MaxLength(500)]
    public string? CancellationNote { get; set; }
}
