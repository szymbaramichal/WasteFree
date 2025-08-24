using System.Net;
using Microsoft.EntityFrameworkCore;
using WasteFree.Business.Abstractions.Messaging;
using WasteFree.Business.Helpers;
using WasteFree.Infrastructure;
using WasteFree.Shared.Constants;
using WasteFree.Shared.Entities;
using WasteFree.Shared.Enums;
using WasteFree.Shared.Models;

namespace WasteFree.Business.Features.Auth;

public record RegisterUserCommand(string Email, string Username, string Password) : IRequest<UserDto>;

public class RegisterUserCommandHandler(ApplicationDataContext context) : IRequestHandler<RegisterUserCommand, UserDto>
{
    public async Task<Result<UserDto>> HandleAsync(RegisterUserCommand command, CancellationToken cancellationToken)
    {
        var userByUsername = await context.Users.FirstOrDefaultAsync(x => x.Username.ToLower() == command.Username.ToLower(), cancellationToken);
        
        if(userByUsername is not null)
            return Result<UserDto>.Failure(ApiErrorCodes.UsernameTaken, HttpStatusCode.BadRequest);
        
        var userByEmail = await context.Users.FirstOrDefaultAsync(x => x.Email.ToLower() == command.Email.ToLower(), cancellationToken);
        
        if(userByEmail is not null)
            return Result<UserDto>.Failure(ApiErrorCodes.EmailTaken, HttpStatusCode.BadRequest);
        
        var hashAndSalt = PasswordHasher.GeneratePasswordHashAndSalt(command.Password);
        
        var newUser = new User {
            Email = command.Email,
            PasswordHash = hashAndSalt.passwordHash,
            PasswordSalt = hashAndSalt.passwordSalt,
            Username = command.Username,
            Role = UserRole.User
        };
        
        context.Users.Add(newUser);
        await context.SaveChangesAsync(cancellationToken);

        return Result<UserDto>.Success(newUser.MapToUserDto());
    }
}