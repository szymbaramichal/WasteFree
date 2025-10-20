using WasteFree.Domain.Entities;
using WasteFree.Domain.Models;

namespace WasteFree.Application.Features.Account.Dtos;

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
    public string? AvatarUrl { get; set; }

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
    /// Address object.
    /// </summary>
    public required Address Address { get; set; }
}

public static class ProfileDtoExtensions
{
    public static ProfileDto MapToProfileDto(this User user, string? avatarUrl = null)
    {
        return new ProfileDto
        {
            UserId = user.Id,
            BankAccountNumber = user.Wallet?.WithdrawalAccountNumber ?? string.Empty,
            Description = user.Description ?? string.Empty,
            Email = user.Email,
            Username = user.Username,
            Address = user.Address,
            AvatarUrl = string.IsNullOrWhiteSpace(avatarUrl) ? null : avatarUrl
        };
    }
    
    public static ProfileDto MapToProfileWithAvatarUrl(this User user, string avatarUrl)
    {
        return user.MapToProfileDto(avatarUrl);
    }
}