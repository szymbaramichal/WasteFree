using WasteFree.Domain.Entities;
using WasteFree.Domain.Enums;

namespace WasteFree.Application.Features;

/// <summary>
/// Data transfer object for a user returned by the API.
/// </summary>
/// <param name="Id">Unique identifier of the user.</param>
/// <param name="Email">User's email address.</param>
/// <param name="Username">User's display name.</param>
/// <param name="Token">JWT token for authenticated requests; null when not issued.</param>
/// <param name="AvatarUrl">Avatar sas url.</param>
/// <param name="UserRole">Role assigned to the user.</param>
/// <param name="AcceptedConsents">Accepted user consents</param>
public record UserDto(
    Guid Id,
    string Email,
    string Username,
    string? Token,
    string? AvatarUrl,
    UserRole UserRole,
    bool AcceptedConsents
);

public static class UserDtoExtensions
{
    public static UserDto MapToUserDto(this User user, string token = "", string? avatarUrl = null)
    {
        return new UserDto(
            user.Id,
            user.Email,
            user.Username,
            string.IsNullOrWhiteSpace(token) ? null : token,
            string.IsNullOrWhiteSpace(avatarUrl) ? null : avatarUrl,
            user.Role,
            user.ConsentsAgreed
        );
    }
}