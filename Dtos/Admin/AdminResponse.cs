namespace BookingSystem.Dtos.Admin;

public class AdminResponse
{
    public Guid AdminId { get; set; }
    public string Username { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}
