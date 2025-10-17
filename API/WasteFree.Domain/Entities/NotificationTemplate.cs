using WasteFree.Domain.Enums;
using WasteFree.Domain.Models;

namespace WasteFree.Domain.Entities;

public class NotificationTemplate : DatabaseEntity
{
    public required string Subject { get; set; }
    public required string Body { get; set; }
    public required NotificationChannel Channel { get; set; }
    public required NotificationType Type { get; set; }
    public required LanguagePreference LanguagePreference { get; set; }
}