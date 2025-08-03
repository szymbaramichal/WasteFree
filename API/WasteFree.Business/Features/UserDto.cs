using WasteFree.Shared.Entities;

namespace WasteFree.Business.Features;

public class UserDto
{

    public static UserDto MapTo(User user)
    {
        return new UserDto();
    }
}