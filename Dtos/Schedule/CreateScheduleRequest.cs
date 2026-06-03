using System.ComponentModel.DataAnnotations;

namespace BookingSystem.Dtos.Schedule;

public class CreateScheduleRequest
{
    [Required]
    public DateOnly Date { get; set; }

    [Required]
    public TimeOnly StartTime { get; set; }

    [Required]
    public TimeOnly EndTime { get; set; }
}
