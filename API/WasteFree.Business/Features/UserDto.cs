using WasteFree.Shared.Entities;

namespace WasteFree.Business.Features;

/// <summary>
/// Data transfer object for a user returned by the API.
/// </summary>
/// <param name="Id">Unique identifier of the user.</param>
/// <param name="Email">User's email address.</param>
/// <param name="Username">User's display name.</param>
/// <param name="Token">JWT token for authenticated requests; null when not issued.</param>
/// <param name="UserRole">Role assigned to the user.</param>
public record UserDto(
    Guid Id,
    string Email,
    string Username,
    string? Token,
    string UserRole
);

public static class UserDtoExtensions
{
    public static UserDto MapToUserDto(this User user, string token = "")
    {
        return new UserDto(
            user.Id,
            user.Email,
            user.Username,
            token,
            user.Role.ToString()
        );
    }
}