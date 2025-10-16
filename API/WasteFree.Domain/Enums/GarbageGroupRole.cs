namespace WasteFree.Shared.Enums;

/// <summary>
/// Role of a user within a garbage group.
/// </summary>
public enum GarbageGroupRole
{
    /// <summary>
    /// The owner of the garbage group with administrative permissions.
    /// </summary>
    Owner = 1,

    /// <summary>
    /// A regular group member.
    /// </summary>
    User
}