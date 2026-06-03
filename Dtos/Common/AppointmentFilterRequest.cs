namespace BookingSystem.Dtos.Common;

public class AppointmentFilterRequest : PaginationRequest
{
    public bool TodayOnly { get; set; } = false;
}
