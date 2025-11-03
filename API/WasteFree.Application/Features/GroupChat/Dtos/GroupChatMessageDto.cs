using WasteFree.Domain.Entities;

namespace WasteFree.Application.Features.GroupChat.Dtos;

public class GroupChatMessageDto
{
    public Guid Id { get; set; }
    public Guid GroupId { get; set; }
    public Guid UserId { get; set; }
    public required string Username { get; set; }
    public string? AvatarName { get; set; }
    public required string Content { get; set; }
    public DateTime SentAtUtc { get; set; }
}

public static class GroupChatMessageDtoExtensions
{
    public static GroupChatMessageDto ToDto(this GarbageGroupMessage message, User? author = null)
    {
        var user = author ?? message.User;

        return new GroupChatMessageDto
        {
            Id = message.Id,
            GroupId = message.GarbageGroupId,
            UserId = message.UserId,
            Username = user?.Username ?? string.Empty,
            AvatarName = user?.AvatarName,
            Content = message.Content,
            SentAtUtc = message.CreatedDateUtc
        };
    }
}
