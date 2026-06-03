namespace BookingSystem.Entities;

public class Admin : BaseDateTime
{
    public Guid AdminId { get; set; }
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
}
