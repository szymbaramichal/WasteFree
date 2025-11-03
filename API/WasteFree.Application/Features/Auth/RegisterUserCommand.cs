using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using WasteFree.Application.Abstractions.Messaging;
using WasteFree.Application.Helpers;
using WasteFree.Application.Jobs;
using WasteFree.Application.Jobs.Dtos;
using WasteFree.Application.Notifications.Facades;
using WasteFree.Infrastructure;
using WasteFree.Domain.Constants;
using WasteFree.Domain.Entities;
using WasteFree.Domain.Enums;
using WasteFree.Domain.Interfaces;
using WasteFree.Domain.Models;

namespace WasteFree.Application.Features.Auth;

public record RegisterUserCommand(string Email, string Username, string Password, string Role, 
    string LanguagePreference, Address Address) 
    : IRequest<UserDto>;

public class RegisterUserCommandHandler(ApplicationDataContext context, 
    IJobSchedulerFacade jobScheduler,
    IConfiguration configuration,
    RegisterUserNotificationFacade notificationFacade) 
    : IRequestHandler<RegisterUserCommand, UserDto>
{
    public async Task<Result<UserDto>> HandleAsync(RegisterUserCommand command, CancellationToken cancellationToken)
    {
        var userByUsername = await context.Users
            .FirstOrDefaultAsync(x => x.Username.ToLower() == command.Username.ToLower(), cancellationToken);
        
        if(userByUsername is not null)
            return Result<UserDto>.Failure(ApiErrorCodes.UsernameTaken, HttpStatusCode.BadRequest);
        
        var userByEmail = await context.Users
            .FirstOrDefaultAsync(x => x.Email.ToLower() == command.Email.ToLower(), cancellationToken);
        
        if(userByEmail is not null)
            return Result<UserDto>.Failure(ApiErrorCodes.EmailTaken, HttpStatusCode.BadRequest);
        
        var hashAndSalt = PasswordHasher.GeneratePasswordHashAndSalt(command.Password);
        
        var newUser = new User {
            Id = Guid.CreateVersion7(),
            Email = command.Email,
            PasswordHash = hashAndSalt.passwordHash,
            PasswordSalt = hashAndSalt.passwordSalt,
            Username = command.Username,
            Role = Enum.TryParse<UserRole>(command.Role, true, out var role) 
                ? role : UserRole.User,
            LanguagePreference= Enum.TryParse<LanguagePreference>(command.LanguagePreference, true, out var language) 
                ? language : LanguagePreference.English,
            Address = command.Address
        };
        
        // Set false for garbageAdmin
        newUser.ConsentsAgreed = newUser.Role == UserRole.User;
        
        context.Users.Add(newUser);
        
        var baseUrl = configuration["BaseUiUrl"] ?? throw new NotImplementedException();
        var aesToken = configuration["Security:AesEncryptionKey"] ?? throw new NotImplementedException();
        var activationLink = $"{baseUrl}/activate-account/{AesEncryptor.Encrypt(newUser.Id.ToString(), aesToken)}";

        var emailContent = await notificationFacade.CreateAsync(
            newUser.LanguagePreference,
            newUser.Username,
            activationLink,
            cancellationToken);

        if (emailContent is null)
            return Result<UserDto>.Failure(ApiErrorCodes.GenericError, HttpStatusCode.BadRequest);

        await context.SaveChangesAsync(cancellationToken);

        await jobScheduler.ScheduleOneTimeJobAsync(nameof(OneTimeJobs.SendEmailJob), 
            new SendEmailDto
            {
                Email = newUser.Email,
                Subject = emailContent.Subject,
                Body = emailContent.Body
            },
            "Send registration confirmation email", 
            cancellationToken);

        return Result<UserDto>.Success(newUser.MapToUserDto());
    }
}