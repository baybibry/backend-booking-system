namespace BookingSystem.Dtos.Schedule;

public class UpdateScheduleRequest
{
    public DateOnly? Date { get; set; }
    public TimeOnly? StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }
}
