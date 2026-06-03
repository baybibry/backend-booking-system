using BookingSystem.Dtos.Notification;
using BookingSystem.Enums;
using BookingSystem.Repositories.Interfaces;
using BookingSystem.Services.Interfaces;

namespace BookingSystem.Services.Implementations;

public class NotificationService(INotificationRepository notificationRepo) : INotificationService
{
    public async Task<List<NotificationResponse>> GetByUserAsync(
        Guid userId,
        UserRole userType,
        CancellationToken ct = default
    ) => await notificationRepo.GetByUserAsync(userId, userType, ct);

    public async Task MarkAsReadAsync(
        Guid userId,
        Guid notificationId,
        CancellationToken ct = default
    )
    {
        var notification = await notificationRepo.GetByIdAsync(notificationId, ct)
            ?? throw new KeyNotFoundException("Notification not found.");

        if (notification.UserId != userId)
            throw new UnauthorizedAccessException("You are not authorized to modify this notification.");

        await notificationRepo.MarkAsReadAsync(notificationId, ct);
    }

    public async Task MarkAllAsReadAsync(
        Guid userId,
        UserRole userType,
        CancellationToken ct = default
    ) => await notificationRepo.MarkAllAsReadAsync(userId, userType, ct);

    public async Task DeleteAsync(
        Guid userId,
        Guid notificationId,
        CancellationToken ct = default
    )
    {
        var notification = await notificationRepo.GetByIdAsync(notificationId, ct)
            ?? throw new KeyNotFoundException("Notification not found.");

        if (notification.UserId != userId)
            throw new UnauthorizedAccessException("You are not authorized to delete this notification.");

        await notificationRepo.DeleteAsync(notificationId, ct);
    }

    public async Task DeleteAllReadAsync(
        Guid userId,
        UserRole userType,
        CancellationToken ct = default
    ) => await notificationRepo.DeleteAllReadAsync(userId, userType, ct);
}
