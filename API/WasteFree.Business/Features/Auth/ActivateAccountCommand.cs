using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using WasteFree.Business.Abstractions.Messaging;
using WasteFree.Business.Features.Auth.Dtos;
using WasteFree.Business.Helpers;
using WasteFree.Infrastructure;
using WasteFree.Shared.Constants;
using WasteFree.Shared.Models;

namespace WasteFree.Business.Features.Auth;

public record ActivateAccountCommand(string AesToken) : IRequest<ActivateAccountDto>;

public class ActivateAccountCommandHandler(ApplicationDataContext context, IConfiguration configuration) 
    : IRequestHandler<ActivateAccountCommand, ActivateAccountDto>
{
    public async Task<Result<ActivateAccountDto>> HandleAsync(ActivateAccountCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var decryptedToken = AesEncryptor.Decrypt(request.AesToken, 
                configuration["Security:AesEncryptionKey"] ?? throw new NotImplementedException());
            if (string.IsNullOrEmpty(decryptedToken))
                return Result<ActivateAccountDto>.Failure(ApiErrorCodes.InvalidRegistrationToken, HttpStatusCode.BadRequest);
        
            var tokenBytes = Convert.FromBase64String(decryptedToken);
            var userId = System.Text.Encoding.UTF8.GetString(tokenBytes);
            
            var user = await context.Users.FirstOrDefaultAsync(x => x.Id.ToString() == userId, cancellationToken);
            
            if(user is null)
                return Result<ActivateAccountDto>.Failure(ApiErrorCodes.InvalidRegistrationToken, HttpStatusCode.BadRequest);

            if(user.IsActive)
                return Result<ActivateAccountDto>.Failure(ApiErrorCodes.InvalidRegistrationToken, HttpStatusCode.BadRequest);

            user.IsActive = true;
            
            await context.SaveChangesAsync(cancellationToken);
            
            return Result<ActivateAccountDto>.Success(new ActivateAccountDto
            {
                Id = user.Id
            });
        }
        catch (Exception e)
        {
            return Result<ActivateAccountDto>.Failure(ApiErrorCodes.GenericError, HttpStatusCode.BadRequest);
        }
    }
}