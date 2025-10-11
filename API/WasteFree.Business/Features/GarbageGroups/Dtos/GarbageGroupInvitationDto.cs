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
    public string GroupName { get; set; } = string.Empty;
    
    /// <summary>
    /// Username of person who sent invitation.
    /// </summary>
    public string InvitingUsername { get; set; } = string.Empty;
    
    /// <summary>
    /// City where the group is located.
    /// </summary>
    public string City { get; set; } = string.Empty;

    /// <summary>
    /// Postal code tied to the group's address.
    /// </summary>
    public string PostalCode { get; set; } = string.Empty;

    /// <summary>
    /// Street address of the group.
    /// </summary>
    public string Address { get; set; } = string.Empty;
}