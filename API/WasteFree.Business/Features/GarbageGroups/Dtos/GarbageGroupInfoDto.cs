namespace WasteFree.Business.Features.GarbageGroups.Dtos;

/// <summary>
/// Lightweight DTO containing basic info about a garbage group.
/// </summary>
public class GarbageGroupInfoDto
{
    /// <summary>
    /// Unique identifier of the garbage group.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Display name of the garbage group.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Indicates whether the current user is the owner of the group.
    /// </summary>
    public bool IsUserOwner { get; set; }
    
    /// <summary>
    /// City where the garbage group is located.
    /// </summary>
    public string City { get; set; }
}