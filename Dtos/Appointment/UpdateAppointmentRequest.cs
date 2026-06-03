using System.ComponentModel.DataAnnotations;
using BookingSystem.Enums;

namespace BookingSystem.Dtos.Appointment;

public class UpdateAppointmentRequest
{
    public AppointmentStatus? Status { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    [MaxLength(1000)]
    public string? Diagnosis { get; set; }

    [MaxLength(500)]
    public string? CancellationNote { get; set; }
}
