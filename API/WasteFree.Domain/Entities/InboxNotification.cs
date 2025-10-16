using WasteFree.Domain.Enums;
using WasteFree.Domain.Models;

namespace WasteFree.Domain.Entities;

public class InboxNotification : DatabaseEntity
{
    public required Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public required string Title { get; set; }
    public required string Message { get; set; }

    public Guid? RelatedEntityId { get; set; } 
    public required InboxActionType ActionType { get; set; } = Enums.InboxActionType.None;
}