using System.Net;
using Microsoft.EntityFrameworkCore;
using WasteFree.Application.Abstractions.Messaging;
using WasteFree.Domain.Constants;
using WasteFree.Domain.Models;
using WasteFree.Infrastructure;

namespace WasteFree.Application.Features.Account;

public record AcceptGarbageAdminConsentCommand(Guid UserId) : IRequest<bool>;

public class AcceptGarbageAdminConsentCommandHandler(ApplicationDataContext context) 
    : IRequestHandler<AcceptGarbageAdminConsentCommand, bool>
{
    public async Task<Result<bool>> HandleAsync(AcceptGarbageAdminConsentCommand request, 
        CancellationToken cancellationToken)
    {
        var updatedCount = await context
            .Users
            .Where(x => x.Id == request.UserId)
            .ExecuteUpdateAsync(x => 
                x.SetProperty(y => y.ConsentsAgreed, true), cancellationToken);

        if (updatedCount > 0)
        {
            return Result<bool>.Success(true);
        }

        return Result<bool>.Failure(ApiErrorCodes.InvalidUser, HttpStatusCode.NotFound);
    }
}
