using WasteFree.Domain.Enums;

namespace WasteFree.Application.Notifications.Models;

public sealed record NotificationMessage(NotificationChannel Channel, string Subject, string Body);
