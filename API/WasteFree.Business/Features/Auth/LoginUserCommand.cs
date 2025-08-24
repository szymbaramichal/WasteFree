using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using WasteFree.Business.Abstractions.Messaging;
using WasteFree.Business.Helpers;
using WasteFree.Infrastructure;
using WasteFree.Shared.Constants;
using WasteFree.Shared.Models;

namespace WasteFree.Business.Features.Auth;

public record LoginUserCommand(string Username, string Password) : IRequest<UserDto>;

public class LoginUserCommandHandler(ApplicationDataContext context, IConfiguration configuration) : IRequestHandler<LoginUserCommand, UserDto>
{
    public async Task<Result<UserDto>> HandleAsync(LoginUserCommand command, CancellationToken cancellationToken)
    {
        var user = await context.Users
            .FirstOrDefaultAsync(x => x.Username.ToLower() == command.Username.ToLower(), cancellationToken);
        
        if (user is null)
            return Result<UserDto>.Failure(ApiErrorCodes.LoginFailed, HttpStatusCode.BadRequest);
        
        var isPasswordValid = PasswordHasher.IsPasswordValid(command.Password, user.PasswordHash, user.PasswordSalt);
        
        if(!isPasswordValid)
            return Result<UserDto>.Failure(ApiErrorCodes.LoginFailed, HttpStatusCode.BadRequest);

        var token = TokenHelper.GenerateJwtToken(user.Username, user.Id.ToString(), (int)user.Role,
            configuration["Security:Jwt:Key"]);

        var dto = user.MapToUserDto(token);

        return Result<UserDto>.Success(dto);
    }
}