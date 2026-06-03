using BookingSystem.Data;
using BookingSystem.Dtos.Notification;
using BookingSystem.Entities;
using BookingSystem.Enums;
using BookingSystem.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Repositories.Implementations;

public class NotificationRepository(BookingContext context) : INotificationRepository
{
    public async Task<List<NotificationResponse>> GetByUserAsync(Guid userId, UserRole userType, CancellationToken ct = default) =>
        await context.Notifications
            .Where(n => n.UserId == userId && n.UserType == userType)
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new NotificationResponse
            {
                NotificationId = n.NotificationId,
                Type = n.Type,
                Subject = n.Subject,
                Content = n.Content,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt
            })
            .AsNoTracking()
            .ToListAsync(ct);

    public async Task<Notification> CreateAsync(Notification notification, CancellationToken ct = default)
    {
        notification.CreatedAt = DateTime.UtcNow;
        notification.UpdatedAt = DateTime.UtcNow;
        context.Notifications.Add(notification);
        await context.SaveChangesAsync(ct);
        return notification;
    }
    
    public async Task<Notification?> GetByIdAsync(Guid notificationId, CancellationToken ct = default) =>
        await context.Notifications.FirstOrDefaultAsync(n => n.NotificationId == notificationId, ct);

    public async Task<Notification> MarkAsReadAsync(Guid notificationId, CancellationToken ct = default)
    {
        var notification = await context.Notifications.FirstOrDefaultAsync(n => n.NotificationId == notificationId, ct)
            ?? throw new KeyNotFoundException("Notification not found.");

        notification.IsRead = true;
        notification.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(ct);
        return notification;
    }

    public async Task MarkAllAsReadAsync(Guid userId, UserRole userType, CancellationToken ct = default)
    {
        await context.Notifications
            .Where(n => n.UserId == userId && n.UserType == userType && !n.IsRead)
            .ExecuteUpdateAsync(s => s
                .SetProperty(n => n.IsRead, true)
                .SetProperty(n => n.UpdatedAt, DateTime.UtcNow), ct);
    }

    public async Task DeleteAsync(Guid notificationId, CancellationToken ct = default)
    {
        var notification = await context.Notifications.FirstOrDefaultAsync(n => n.NotificationId == notificationId, ct)
            ?? throw new KeyNotFoundException("Notification not found.");

        context.Notifications.Remove(notification);
        await context.SaveChangesAsync(ct);
    }

    public async Task DeleteAllReadAsync(Guid userId, UserRole userType, CancellationToken ct = default)
    {
        await context.Notifications
            .Where(n => n.UserId == userId && n.UserType == userType && n.IsRead)
            .ExecuteDeleteAsync(ct);
    }
}
