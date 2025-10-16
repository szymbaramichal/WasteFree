using WasteFree.Domain.Models;

namespace WasteFree.Application.Features.GarbageGroups.Dtos;

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
    /// Street address of the garbage group.
    /// </summary>
    public required Address Address { get; set; }
}