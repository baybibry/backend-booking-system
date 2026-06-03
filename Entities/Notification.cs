using BookingSystem.Enums;

namespace BookingSystem.Entities;

public class Notification : BaseDateTime
{
    public Guid NotificationId { get; set; }
    public UserRole UserType { get; set; }
    public string Subject { get; set; } = null!;
    public string Content { get; set; } = null!;
    public bool IsRead { get; set; }
    public NotificationType Type { get; set; }

    public Guid UserId { get; set; }
}
