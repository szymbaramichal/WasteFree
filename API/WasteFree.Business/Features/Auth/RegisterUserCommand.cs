using System.Net;
using Microsoft.EntityFrameworkCore;
using WasteFree.Business.Abstractions.Messaging;
using WasteFree.Business.Helpers;
using WasteFree.Infrastructure;
using WasteFree.Shared.Entities;
using WasteFree.Shared.Shared;

namespace WasteFree.Business.Features.Auth;

public record RegisterUserCommand(string Email, string Username, string Password) : ICommand<UserDto>;

public class RegisterUserCommandHandler(ApplicationDataContext context) : ICommandHandler<RegisterUserCommand, UserDto>
{
    public async Task<Result<UserDto>> Handle(RegisterUserCommand command, CancellationToken cancellationToken)
    {
        var user = await context.Users.FirstOrDefaultAsync(x => x.Email.ToLower() == command.Email.ToLower(), cancellationToken);
        
        if(user is not null)
            return new Result<UserDto>("User already exists", HttpStatusCode.BadRequest);
        
        var hashAndSalt = PasswordHasher.GeneratePasswordHashAndSalt(command.Password);
        
        var newUser = new User {
            Email = command.Email,
            PasswordHash = hashAndSalt.passwordHash,
            PasswordSalt = hashAndSalt.passwordSalt,
            Username = command.Username
        };
        
        context.Users.Add(newUser);
        await context.SaveChangesAsync(cancellationToken);

        return new Result<UserDto>(newUser.MapToUserDto());
    }
}