using WasteFree.Shared.Entities;

namespace WasteFree.Business.Features.GarbageGroups.Dtos;

public class GarbageGroupDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
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
            Users = garbageGroupUsers
        };
    }
}