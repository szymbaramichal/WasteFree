using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using WasteFree.Business.Abstractions.Messaging;
using WasteFree.Business.Helpers;
using WasteFree.Infrastructure;
using WasteFree.Shared.Shared;

namespace WasteFree.Business.Features.Auth;

public record LoginUserCommand(string Username, string Password) : ICommand<UserDto>;

public class LoginUserCommandHandler(ApplicationDataContext context, IConfiguration configuration) : ICommandHandler<LoginUserCommand, UserDto>
{
    public async Task<Result<UserDto>> Handle(LoginUserCommand command, CancellationToken cancellationToken)
    {
        var user = await context.Users
            .FirstOrDefaultAsync(x => x.Username == command.Username, cancellationToken);
        
        if (user is null)
            return new Result<UserDto>("Login or password is incorrect", HttpStatusCode.BadRequest);
        
        var isPasswordValid = PasswordHasher.IsPasswordValid(command.Password, user.PasswordHash, user.PasswordSalt);
        
        if(!isPasswordValid)
            return new Result<UserDto>("Login or password is incorrect", HttpStatusCode.BadRequest);
            
        var token = TokenHelper.GenerateJwtToken(user.Username, 
                            configuration["Security:Jwt:Key"] ?? throw new InvalidOperationException());

        var dto = user.MapToUserDto(token);
        
        return new Result<UserDto>(dto);
    }
}