namespace WasteFree.Domain.Enums;

/// <summary>
/// High-level classification of notifications used in the system.
/// </summary>
public enum NotificationType
{
    /// <summary>
    /// Registration confirmation.
    /// </summary>
    RegisterationConfirmation = 0,
    
    /// <summary>
    /// Password reset.
    /// </summary>
    PasswordReset = 1,
    
    /// <summary>
    /// Garbage Group invitation.
    /// </summary>
    GarbageGroupInvitation = 2
}