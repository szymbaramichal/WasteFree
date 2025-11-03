using WasteFree.Domain.Models;

namespace WasteFree.Application.Features.GarbageGroups.Dtos;

/// <summary>
/// Garbage group with users 
/// </summary>
public class GarbageGroupMembersDto
{
    /// <summary>
    /// Group id
    /// </summary>
    public Guid GroupId { get; set; }

    /// <summary>
    /// Garbage group users collection
    /// </summary>
    public required ICollection<GarbageGroupUserDto> GroupUsers { get; set; }

    /// <summary>
    /// Is group only for user
    /// </summary>
    public bool IsPrivate { get; set; }


    /// <summary>
    /// Group address
    /// </summary>
    public required Address Address { get; set; }
}