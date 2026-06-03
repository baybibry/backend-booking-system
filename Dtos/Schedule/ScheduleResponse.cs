namespace BookingSystem.Dtos.Schedule;

public class ScheduleResponse
{
    public Guid ScheduleId { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public bool IsBooked { get; set; }
    public bool IsBlocked { get; set; }
}
