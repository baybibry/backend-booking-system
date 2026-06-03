using System.ComponentModel.DataAnnotations;

namespace BookingSystem.Dtos.Schedule;

public class ReceptionistCreateScheduleRequest
{
    [Required]
    public Guid DoctorId { get; set; }

    [Required]
    public DateOnly Date { get; set; }

    [Required]
    public TimeOnly StartTime { get; set; }

    [Required]
    public TimeOnly EndTime { get; set; }
}
