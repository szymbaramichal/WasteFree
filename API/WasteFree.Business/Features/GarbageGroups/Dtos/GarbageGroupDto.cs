using WasteFree.Shared.Entities;

namespace WasteFree.Business.Features.GarbageGroups.Dtos;

/// <summary>
/// DTO representing a garbage group with its metadata and members.
/// </summary>
public class GarbageGroupDto
{
    /// <summary>
    /// Unique identifier of the garbage group.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Display name of the group.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Description or additional information about the group.
    /// </summary>
    public required string Description { get; set; }

    /// <summary>
    /// City where the garbage group is located.
    /// </summary>
    public required string City { get; set; }

    
    /// <summary>
    /// Collection of users that belong to this group.
    /// </summary>
    public ICollection<GarbageGroupUserDto> Users { get; set; } = [];
}

public static class GarbageGroupDtoExtensions
{
    public static GarbageGroupDto MapToGarbageGroupDto(this GarbageGroup garbageGroup,
        ICollection<UserGarbageGroup> users)
    {
        var garbageGroupUsers = users.MapToGarbageGroupUserDto();

        return new GarbageGroupDto
        {
            Description = garbageGroup.Description,
            Name = garbageGroup.Name,
            Id = garbageGroup.Id,
            Users = garbageGroupUsers,
            City = garbageGroup.City
        };
    }
}