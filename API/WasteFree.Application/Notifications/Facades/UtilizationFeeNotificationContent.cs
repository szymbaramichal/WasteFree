using WasteFree.Application.Notifications.Models;

namespace WasteFree.Application.Notifications.Facades;

public sealed record UtilizationFeeNotificationContent(
    Guid UserId,
    NotificationMessage? Email,
    NotificationMessage? Inbox);
