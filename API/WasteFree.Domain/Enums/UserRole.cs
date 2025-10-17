namespace WasteFree.Domain.Enums;

/// <summary>
/// Global user roles used for authorization and role-based UI behavior.
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Regular user with standard permissions.
    /// </summary>
    User = 1,

    /// <summary>
    /// User responsible for managing garbage-related administrative tasks.
    /// </summary>
    GarbageAdmin,

    /// <summary>
    /// System administrator with full privileges.
    /// </summary>
    Admin
}