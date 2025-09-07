using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using WasteFree.Business.Abstractions.Messaging;
using WasteFree.Business.Features.Auth.Dtos;
using WasteFree.Business.Helpers;
using WasteFree.Infrastructure;
using WasteFree.Shared.Constants;
using WasteFree.Shared.Entities;
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

            var userId = Guid.Parse(decryptedToken);
        
            var user = await context.Users.FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
            
            if(user is null || user.IsActive)
                return Result<ActivateAccountDto>.Failure(ApiErrorCodes.InvalidRegistrationToken, HttpStatusCode.BadRequest);

            user.IsActive = true;
            var wallet = new Wallet
            {
                UserId = user.Id,
                Funds = 0.00
            };
            
            await context.Wallets.AddAsync(wallet, cancellationToken);
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