using WasteFree.Domain.Models;

namespace WasteFree.Application.Features.GarbageGroups.Dtos;

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
    /// Address object.
    /// </summary>
    public required Address Address { get; set; }
}