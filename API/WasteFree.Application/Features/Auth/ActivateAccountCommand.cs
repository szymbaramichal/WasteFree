using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using WasteFree.Application.Abstractions.Messaging;
using WasteFree.Application.Features.Auth.Dtos;
using WasteFree.Application.Helpers;
using WasteFree.Infrastructure;
using WasteFree.Domain.Constants;
using WasteFree.Domain.Entities;
using WasteFree.Domain.Enums;
using WasteFree.Domain.Models;

namespace WasteFree.Application.Features.Auth;

public record ActivateAccountCommand(string AesToken) : IRequest<ActivateAccountDto>;

public class ActivateAccountCommandHandler(ApplicationDataContext context, IConfiguration configuration) 
    : IRequestHandler<ActivateAccountCommand, ActivateAccountDto>
{
    public async Task<Result<ActivateAccountDto>> HandleAsync(ActivateAccountCommand request, CancellationToken cancellationToken)
    {
        // Simulate processing
        await Task.Delay(500, cancellationToken);
        
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

            var wallet = new WasteFree.Domain.Entities.Wallet
            {
                UserId = user.Id,
                Funds = 0.00
            };

            var privateGroup = new GarbageGroup
            {
                Id = Guid.CreateVersion7(),
                Name = $"{user.Username}",
                Description = $"Private garbage group for {user.Username}",
                Address = user.Address,
                IsPrivate = true
            };

            var privateMembership = new UserGarbageGroup
            {
                Id = Guid.CreateVersion7(),
                UserId = user.Id,
                GarbageGroupId = privateGroup.Id,
                Role = GarbageGroupRole.Owner,
                IsPending = false
            };

            await context.Wallets.AddAsync(wallet, cancellationToken);
            await context.GarbageGroups.AddAsync(privateGroup, cancellationToken);
            await context.UserGarbageGroups.AddAsync(privateMembership, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            
            return Result<ActivateAccountDto>.Success(new ActivateAccountDto
            {
                Id = user.Id
            });
        }
        catch (Exception)
        {
            return Result<ActivateAccountDto>.Failure(ApiErrorCodes.GenericError, HttpStatusCode.BadRequest);
        }
    }
}