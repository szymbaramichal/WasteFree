namespace WasteFree.Shared.Enums;

/// <summary>
/// Channels through which notifications can be delivered.
/// </summary>
public enum NotificationChannel
{
    /// <summary>
    /// Notification delivered via email.
    /// </summary>
    Email = 0,

    /// <summary>
    /// Notification delivered to the in-app inbox.
    /// </summary>
    Inbox
}