using WasteFree.Domain.Entities;
using WasteFree.Domain.Enums;

namespace WasteFree.Application.Features.GarbageGroups.Dtos;

/// <summary>
/// DTO representing a user that is a member of a garbage group.
/// </summary>
public class GarbageGroupUserDto
{
    /// <summary>
    /// Identifier of the user.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Username of the user.
    /// </summary>
    public required string Username { get; set; }

    /// <summary>
    /// The role of the user within the garbage group (for example Owner or Member).
    /// </summary>
    public GarbageGroupRole GarbageGroupRole { get; set; }

    /// <summary>
    /// Indicates whether the user's membership is pending (invited but not accepted).
    /// </summary>
    public bool IsPending { get; set; }
}

public static class GarbageGroupUserDtoExtensions
{
    public static ICollection<GarbageGroupUserDto> MapToGarbageGroupUserDto(this ICollection<UserGarbageGroup> users)
    {
        var usersList = new List<GarbageGroupUserDto>();

        foreach (var user in users)
        {
            usersList.Add(new GarbageGroupUserDto
            {
                Id = user.UserId,
                Username = user.User?.Username ?? string.Empty,
                GarbageGroupRole = user.Role,
                IsPending = user.IsPending
            });
        }

        return usersList;
    }
}