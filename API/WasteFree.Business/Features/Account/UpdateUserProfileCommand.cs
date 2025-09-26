using System.Net;
using Microsoft.EntityFrameworkCore;
using WasteFree.Business.Abstractions.Messaging;
using WasteFree.Business.Features.Account.Dtos;
using WasteFree.Infrastructure;
using WasteFree.Shared.Constants;
using WasteFree.Shared.Models;

namespace WasteFree.Business.Features.Account;

public record UpdateUserProfileCommand(Guid UserId, string Description, string BankAccountNumber) :
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

        await context.SaveChangesAsync(cancellationToken);
        
        return Result<ProfileDto>.Success(user.MapToProfileDto());
    }
}