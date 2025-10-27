using System.Net;
using Microsoft.EntityFrameworkCore;
using WasteFree.Application.Abstractions.Messaging;
using WasteFree.Application.Features.Account.Dtos;
using WasteFree.Infrastructure;
using WasteFree.Domain.Constants;
using WasteFree.Domain.Enums;
using WasteFree.Domain.Models;

namespace WasteFree.Application.Features.Account;

public record UpdateUserProfileCommand(Guid UserId, 
    string Description, 
    string BankAccountNumber, 
    Address Address,
    PickupOption[]? PickupOptions) :
    IRequest<ProfileDto>;

public class UpdateUserProfileCommandHandler(ApplicationDataContext context) : IRequestHandler<UpdateUserProfileCommand, ProfileDto>
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
        user.PickupOptionsList = request.PickupOptions;
        
        await context.SaveChangesAsync(cancellationToken);

        return Result<ProfileDto>.Success(user.MapToProfileDto());
    }
}