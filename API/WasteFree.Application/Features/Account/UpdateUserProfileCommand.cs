using System;
using System.Net;
using Microsoft.EntityFrameworkCore;
using WasteFree.Application.Abstractions.Messaging;
using WasteFree.Application.Features.Account.Dtos;
using WasteFree.Infrastructure;
using WasteFree.Domain.Constants;
using WasteFree.Domain.Interfaces;
using WasteFree.Domain.Models;

namespace WasteFree.Application.Features.Account;

public record UpdateUserProfileCommand(Guid UserId, string Description, string BankAccountNumber, Address Address) :
    IRequest<ProfileDto>;

public class UpdateUserProfileCommandHandler(ApplicationDataContext context, IBlobStorageService blobStorageService) : IRequestHandler<UpdateUserProfileCommand, ProfileDto>
{
    public async Task<Result<ProfileDto>> HandleAsync(UpdateUserProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await context.Users
            .Include(x => x.Wallet)
            .FirstOrDefaultAsync(x => x.Id == request.UserId, cancellationToken);
        
        if (user is null)
            return Result<ProfileDto>.Failure(ApiErrorCodes.GenericError, HttpStatusCode.BadRequest);

        user.Description = request.Description;
        user.Wallet.WithdrawalAccountNumber = request.BankAccountNumber;
        user.Address = request.Address;

        await context.SaveChangesAsync(cancellationToken);

        string? avatarUrl = null;

        if (!string.IsNullOrWhiteSpace(user.AvatarName))
        {
            avatarUrl = await blobStorageService.GetReadSasUrlAsync(
                BlobContainerNames.Avatars,
                user.AvatarName,
                TimeSpan.FromMinutes(5),
                cancellationToken);
        }

        return Result<ProfileDto>.Success(user.MapToProfileDto(avatarUrl));
    }
}