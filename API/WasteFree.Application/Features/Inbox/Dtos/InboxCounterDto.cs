namespace WasteFree.Business.Features.Inbox.Dtos;

/// <summary>
/// DTO representing counts for the user's inbox (for example unread message count).
/// </summary>
public class InboxCounterDto
{
    /// <summary>
    /// Number of unread messages available in the inbox.
    /// </summary>
    public int UnreadMessages { get; set; }
}