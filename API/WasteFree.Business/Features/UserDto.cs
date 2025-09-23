using WasteFree.Shared.Entities;

namespace WasteFree.Business.Features;

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