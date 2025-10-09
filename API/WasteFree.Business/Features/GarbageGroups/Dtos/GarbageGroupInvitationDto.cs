namespace WasteFree.Business.Features.GarbageGroups.Dtos;

/// <summary>
/// DTO representing invitation to group.
/// </summary>
public class GarbageGroupInvitationDto
{
    /// <summary>
    /// Identifier of the group.
    /// </summary>
    public Guid GroupId { get; set; }
    
    /// <summary>
    /// Group name.
    /// </summary>
    public string GroupName { get; set; }
    
    /// <summary>
    /// Username of person who sent invitation.
    /// </summary>
    public string InvitingUsername { get; set; }
}