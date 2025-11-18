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
    GarbageGroupInvitation = 2,
    
    /// <summary>
    /// Garbage order created.
    /// </summary>
    GarbageOrderCreated = 3,

    /// <summary>
    /// Additional utilization fee is pending from participants.
    /// </summary>
    UtilizationFeePending = 4,

    /// <summary>
    /// Utilization fee has been fully settled for participants.
    /// </summary>
    UtilizationFeeCompletedParticipant = 5,

    /// <summary>
    /// Utilization fee has been fully settled and admin payout is available.
    /// </summary>
    UtilizationFeeCompletedAdmin = 6
}