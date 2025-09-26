using WasteFree.Shared.Enums;
using WasteFree.Shared.Models;

namespace WasteFree.Shared.Entities;

public class InboxNotification : DatabaseEntity
{
    public required Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public required string Title { get; set; }
    public required string Message { get; set; }
    public bool IsRead { get; set; } = false;

    public Guid? RelatedEntityId { get; set; } 
    public required InboxActionType ActionType { get; set; } = Enums.InboxActionType.None;
}