using BookingSystem.Dtos.Notification;
using BookingSystem.Enums;

namespace BookingSystem.Services.Interfaces;

public interface INotificationService
{
    Task<List<NotificationResponse>> GetByUserAsync(
        Guid userId,
        UserRole userType,
        CancellationToken ct = default
    );

    Task MarkAsReadAsync(
        Guid userId,
        Guid notificationId,
        CancellationToken ct = default
    );

    Task MarkAllAsReadAsync(
        Guid userId,
        UserRole userType,
        CancellationToken ct = default
    );

    Task DeleteAsync(
        Guid userId,
        Guid notificationId,
        CancellationToken ct = default
    );

    Task DeleteAllReadAsync(
        Guid userId,
        UserRole userType,
        CancellationToken ct = default
    );
}
