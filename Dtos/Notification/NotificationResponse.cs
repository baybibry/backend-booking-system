using BookingSystem.Enums;

namespace BookingSystem.Dtos.Notification;

public class NotificationResponse
{
    public Guid NotificationId { get; set; }
    public NotificationType Type { get; set; }
    public string Subject { get; set; } = null!;
    public string Content { get; set; } = null!;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}
