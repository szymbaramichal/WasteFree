using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using WasteFree.Business.Abstractions.Messaging;
using WasteFree.Business.Helpers;
using WasteFree.Infrastructure;
using WasteFree.Shared.Constants;
using WasteFree.Shared.Interfaces;
using WasteFree.Shared.Models;

namespace WasteFree.Business.Features.Auth;

public record LoginUserCommand(string Username, string Password) : IRequest<UserDto>;

public class LoginUserCommandHandler(ApplicationDataContext context, 
    IConfiguration configuration, IBlobStorageService blobStorageService) : IRequestHandler<LoginUserCommand, UserDto>
{
    public async Task<Result<UserDto>> HandleAsync(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var user = await context.Users
            .FirstOrDefaultAsync(x => x.Username.ToLower() == request.Username.ToLower(), cancellationToken);
        
        if (user is null)
            return Result<UserDto>.Failure(ApiErrorCodes.LoginFailed, HttpStatusCode.BadRequest);
        
        var isPasswordValid = PasswordHasher.IsPasswordValid(request.Password, user.PasswordHash, user.PasswordSalt);
        
        if(!isPasswordValid)
            return Result<UserDto>.Failure(ApiErrorCodes.LoginFailed, HttpStatusCode.BadRequest);

        if(!user.IsActive)
            return Result<UserDto>.Failure(ApiErrorCodes.UserAccountNotActivated, HttpStatusCode.BadRequest);
        
        var token = TokenHelper.GenerateJwtToken(user.Username, user.Id.ToString(), (int)user.Role,
            configuration["Security:Jwt:Key"]);
        
        var avatarUrl = 
            await blobStorageService.GetReadSasUrlAsync("avatars", user.AvatarName ?? string.Empty, 
                TimeSpan.FromMinutes(5), cancellationToken);

        var dto = user.MapToUserDto(token, avatarUrl);

        return Result<UserDto>.Success(dto);
    }
}