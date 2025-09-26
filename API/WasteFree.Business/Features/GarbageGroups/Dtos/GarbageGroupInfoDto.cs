namespace WasteFree.Business.Features.GarbageGroups.Dtos;

public class GarbageGroupInfoDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public bool IsUserOwner { get; set; }
}