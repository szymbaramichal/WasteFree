using WasteFree.Domain.Enums;

namespace WasteFree.Application.Features.Inbox.Dtos;

/// <summary>
/// DTO representing a single inbox message.
/// </summary>
public class InboxMessageDto
{
    /// <summary>
    /// Unique identifier of the message.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Short title or subject of the message.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Message body content.
    /// </summary>
    public string Body { get; set; } = string.Empty;

    /// <summary>
    /// UTC timestamp when the message was created.
    /// </summary>
    public DateTime CreatedDateUtc { get; set; }

    /// <summary>
    /// Optional action type that defines how the client should react.
    /// </summary>
    public InboxActionType ActionType { get; set; } = InboxActionType.None;

    /// <summary>
    /// Related entity identifier (e.g., group or order) used for contextual redirects.
    /// </summary>
    public Guid? RelatedEntityId { get; set; }
}
