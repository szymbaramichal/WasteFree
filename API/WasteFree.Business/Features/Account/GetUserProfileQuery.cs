using System.Net;
using Microsoft.EntityFrameworkCore;
using WasteFree.Business.Abstractions.Messaging;
using WasteFree.Business.Features.Account.Dtos;
using WasteFree.Infrastructure;
using WasteFree.Shared.Constants;
using WasteFree.Shared.Models;

namespace WasteFree.Business.Features.Account;

public record GetUserProfileQuery(Guid UserId) : IRequest<ProfileDto>;

public class GetUserProfileQueryHandler(ApplicationDataContext context) : IRequestHandler<GetUserProfileQuery, ProfileDto>
{
    public async Task<Result<ProfileDto>> HandleAsync(GetUserProfileQuery request, CancellationToken cancellationToken)
    {
        var user = await context.Users
            .AsNoTracking()
            .Include(x => x.Wallet)
            .FirstOrDefaultAsync(x => x.Id == request.UserId, cancellationToken);
        
        if (user is null)
            return Result<ProfileDto>.Failure(ApiErrorCodes.GenericError, HttpStatusCode.BadRequest);

        await context.SaveChangesAsync(cancellationToken);
        
        return Result<ProfileDto>.Success(new ProfileDto()
        {
            UserId = request.UserId,
            BankAccountNumber =  user.Wallet.WithdrawalAccountNumber ?? string.Empty,
            Description = user.Description ?? string.Empty,
            Email = user.Email,
            Username = user.Username
        });
    }
}