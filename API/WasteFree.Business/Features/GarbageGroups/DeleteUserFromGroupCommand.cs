using System.Net;
using Microsoft.EntityFrameworkCore;
using WasteFree.Business.Abstractions.Messaging;
using WasteFree.Infrastructure;
using WasteFree.Shared.Enums;
using WasteFree.Shared.Models;

namespace WasteFree.Business.Features.GarbageGroups;

public record DeleteUserFromGroupCommand(Guid GroupId, Guid CurrentUserId, Guid UserToRemoveId) : IRequest<bool>;

public class DeleteUserFromGroupCommandHandler(ApplicationDataContext context) : IRequestHandler<DeleteUserFromGroupCommand, bool>
{
    public async Task<Result<bool>> HandleAsync(DeleteUserFromGroupCommand request, CancellationToken cancellationToken)
    {
        var userGroupInfo = await context.UserGarbageGroups
            .Include(x => x.GarbageGroup)
            .FirstOrDefaultAsync(x => x.UserId == request.CurrentUserId && x.GarbageGroupId == request.GroupId
                                                                            && x.Role == GarbageGroupRole.Owner, cancellationToken);
        
        if (userGroupInfo is null)
            return Result<bool>.Failure("NOT_FOUND", HttpStatusCode.NotFound);

        int rows = await context.UserGarbageGroups.Where(x => x.UserId == request.UserToRemoveId)
            .ExecuteDeleteAsync(cancellationToken);

        if(rows > 0)
            return Result<bool>.Success(true);
        
        return Result<bool>.Failure("NOT_FOUND", HttpStatusCode.NotFound);
    }
}