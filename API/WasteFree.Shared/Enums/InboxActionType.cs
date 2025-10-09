namespace WasteFree.Shared.Enums;

/// <summary>
/// Represents the action type associated with an inbox message.
/// </summary>
public enum InboxActionType
{
    /// <summary>
    /// No specific action.
    /// </summary>
    None = 0,

    /// <summary>
    /// The message is an invitation to join a group.
    /// </summary>
    GroupInvitation,

    /// <summary>
    /// The message relates to making a payment.
    /// </summary>
    MakePayment
}