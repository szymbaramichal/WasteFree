using Microsoft.EntityFrameworkCore;
using WasteFree.Application.Abstractions.Messaging;
using WasteFree.Application.Features.GarbageGroups.Dtos;
using WasteFree.Infrastructure;
using WasteFree.Infrastructure.Extensions;
using WasteFree.Domain.Models;

namespace WasteFree.Application.Features.GarbageGroups;

public record GetPendingGroupInvitationsQuery(Guid UserId) : IRequest<ICollection<GarbageGroupInvitationDto>>;

public class GetPendingGroupInvitationsQueryHandler(ApplicationDataContext applicationDataContext) : IRequestHandler<GetPendingGroupInvitationsQuery, ICollection<GarbageGroupInvitationDto>>
{
    public async Task<Result<ICollection<GarbageGroupInvitationDto>>> HandleAsync(GetPendingGroupInvitationsQuery request, CancellationToken cancellationToken)
    {
        var userInvitations = await applicationDataContext.UserGarbageGroups
            .AsNoTracking()
            .FilterNonPrivate()
            .Where(x => x.UserId == request.UserId && x.IsPending)
            .Select(x => new GarbageGroupInvitationDto
            {
                GroupId = x.GarbageGroupId,
                GroupName = x.GarbageGroup.Name,
                Address = x.GarbageGroup.Address
            })
            .ToListAsync(cancellationToken);
        
        foreach (var invitation in userInvitations)
        {
            var invitingUser = await applicationDataContext.UserGarbageGroups
                .FilterNonPrivate()
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.GarbageGroupId == invitation.GroupId 
                                     && x.Role == Domain.Enums.GarbageGroupRole.Owner, cancellationToken);
            
            if (invitingUser != null)
            {
                invitation.InvitingUsername = invitingUser.User.Username ?? "Unknown";
            }
            else
            {
                invitation.InvitingUsername = "Unknown";
            }
        }
        
        return Result<ICollection<GarbageGroupInvitationDto>>.Success(userInvitations);
    }
}