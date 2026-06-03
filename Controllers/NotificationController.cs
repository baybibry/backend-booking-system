using BookingSystem.Dtos.Notification;
using BookingSystem.Enums;
using BookingSystem.Helper;
using BookingSystem.Services.Interfaces;
using BookingSystem.Wrapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingSystem.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationController(INotificationService notificationService) : ControllerBase
{
    // GET /api/notifications
    [HttpGet]
    public async Task<IActionResult> GetNotifications(CancellationToken ct)
    {
        var result = await notificationService.GetByUserAsync(User.GetUserId(), User.GetRole(), ct);
        return Ok(ResponseWrapper<List<NotificationResponse>>.On(
            result,
            "Notifications retrieved.",
            StatusType.Success,
            StatusCodes.Status200OK
        ));
    }

    // PUT /api/notifications/{notificationId}/read
    [HttpPut("{notificationId:guid}/read")]
    public async Task<IActionResult> MarkAsRead(
        Guid notificationId,
        CancellationToken ct
    )
    {
        await notificationService.MarkAsReadAsync(User.GetUserId(), notificationId, ct);
        return Ok(ResponseWrapper<string>.On(
            null,
            "Notification marked as read.",
            StatusType.Success,
            StatusCodes.Status200OK
        ));
    }

    // PUT /api/notifications/read-all
    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken ct)
    {
        await notificationService.MarkAllAsReadAsync(User.GetUserId(), User.GetRole(), ct);
        return Ok(ResponseWrapper<string>.On(
            null,
            "All notifications marked as read.",
            StatusType.Success,
            StatusCodes.Status200OK
        ));
    }

    // DELETE /api/notifications/{notificationId}
    [HttpDelete("{notificationId:guid}")]
    public async Task<IActionResult> Delete(
        Guid notificationId,
        CancellationToken ct
    )
    {
        await notificationService.DeleteAsync(User.GetUserId(), notificationId, ct);
        return Ok(ResponseWrapper<string>.On(
            null,
            "Notification deleted.",
            StatusType.Success,
            StatusCodes.Status200OK
        ));
    }

    // DELETE /api/notifications/read
    [HttpDelete("read")]
    public async Task<IActionResult> DeleteAllRead(CancellationToken ct)
    {
        await notificationService.DeleteAllReadAsync(User.GetUserId(), User.GetRole(), ct);
        return Ok(ResponseWrapper<string>.On(
            null,
            "All read notifications deleted.",
            StatusType.Success,
            StatusCodes.Status200OK
        ));
    }
}
