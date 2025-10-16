using WasteFree.Shared.Enums;
using WasteFree.Shared.Models;

namespace WasteFree.Shared.Entities;

public class NotificationTemplate : DatabaseEntity
{
    public required string Subject { get; set; }
    public required string Body { get; set; }
    public required NotificationChannel Channel { get; set; }
    public required NotificationType Type { get; set; }
    public required LanguagePreference LanguagePreference { get; set; }
}