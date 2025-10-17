using System.Net;
using Microsoft.EntityFrameworkCore;
using WasteFree.Application.Abstractions.Messaging;
using WasteFree.Application.Features.Account.Dtos;
using WasteFree.Infrastructure;
using WasteFree.Domain.Constants;
using WasteFree.Domain.Models;

namespace WasteFree.Application.Features.Account;

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
        
        return Result<ProfileDto>.Success(user.MapToProfileDto());
    }
}