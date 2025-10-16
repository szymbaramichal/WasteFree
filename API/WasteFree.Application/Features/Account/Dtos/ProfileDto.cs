using WasteFree.Shared.Entities;

namespace WasteFree.Business.Features.Account.Dtos;

/// <summary>
/// DTO representing a user's profile information returned by account endpoints.
/// </summary>
public class ProfileDto
{
    /// <summary>
    /// Unique identifier of the user.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// User's chosen username.
    /// </summary>
    public string Username { get; set; } = null!;
    
    /// <summary>
    /// User's avatar sas url
    /// </summary>
    public string AvatarUrl { get; set; } = null!;

    /// <summary>
    /// User's email address.
    /// </summary>
    public string Email { get; set; } = null!;

    /// <summary>
    /// Free-form user description or bio.
    /// </summary>
    public string Description { get; set; } = null!;

    /// <summary>
    /// Bank account number used for withdrawals (if provided).
    /// </summary>
    public string BankAccountNumber { get; set; } = null!;

    /// <summary>
    /// City where the user is located.
    /// </summary>
    public string City { get; set; } = string.Empty;
}

public static class ProfileDtoExtensions
{
    public static ProfileDto MapToProfileDto(this User user)
    {
        return new ProfileDto
        {
            UserId = user.Id,
            BankAccountNumber = user.Wallet?.WithdrawalAccountNumber ?? string.Empty,
            Description = user.Description ?? string.Empty,
            Email = user.Email,
            Username = user.Username,
            City = user.City ?? string.Empty,
        };
    }
    
    public static ProfileDto MapToProfileWithAvatarUrl(this User user, string avatarUrl)
    {
        return new ProfileDto
        {
            UserId = user.Id,
            BankAccountNumber = user.Wallet?.WithdrawalAccountNumber ?? string.Empty,
            Description = user.Description ?? string.Empty,
            Email = user.Email,
            Username = user.Username,
            City = user.City ?? string.Empty,
            AvatarUrl = avatarUrl
        };
    }
}