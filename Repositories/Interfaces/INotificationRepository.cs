using BookingSystem.Dtos.Notification;
using BookingSystem.Entities;
using BookingSystem.Enums;

namespace BookingSystem.Repositories.Interfaces;

public interface INotificationRepository
{
    Task<List<NotificationResponse>> GetByUserAsync(Guid userId, UserRole userType, CancellationToken ct = default);
    Task<Notification?> GetByIdAsync(Guid notificationId, CancellationToken ct = default);
    Task<Notification> MarkAsReadAsync(Guid notificationId, CancellationToken ct = default);
    Task MarkAllAsReadAsync(Guid userId, UserRole userType, CancellationToken ct = default);
    Task DeleteAsync(Guid notificationId, CancellationToken ct = default);
    Task DeleteAllReadAsync(Guid userId, UserRole userType, CancellationToken ct = default);

    Task<Notification> CreateAsync(Notification notification, CancellationToken ct = default);
}
