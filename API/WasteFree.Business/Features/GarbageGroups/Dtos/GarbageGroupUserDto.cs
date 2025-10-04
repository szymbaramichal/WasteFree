using WasteFree.Shared.Entities;
using WasteFree.Shared.Enums;

namespace WasteFree.Business.Features.GarbageGroups.Dtos;

public class GarbageGroupUserDto
{
    public Guid Id { get; set; }
    public required string Username { get; set; }
    public GarbageGroupRole GarbageGroupRole { get; set; }
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
                Id = user.Id,
                Username = user.User?.Username ?? string.Empty,
                GarbageGroupRole = user.Role,
                IsPending = user.IsPending
            });
        }

        return usersList;
    }
}