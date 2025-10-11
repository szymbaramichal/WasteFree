using Microsoft.EntityFrameworkCore;
using WasteFree.Business.Abstractions.Messaging;
using WasteFree.Business.Features.GarbageGroups.Dtos;
using WasteFree.Infrastructure;
using WasteFree.Shared.Models;

namespace WasteFree.Business.Features.GarbageGroups;

public record GetPendingGroupInvitationsQuery(Guid UserId) : IRequest<ICollection<GarbageGroupInvitationDto>>;

public class GetPendingGroupInvitationsQueryHandler(ApplicationDataContext applicationDataContext) : IRequestHandler<GetPendingGroupInvitationsQuery, ICollection<GarbageGroupInvitationDto>>
{
    public async Task<Result<ICollection<GarbageGroupInvitationDto>>> HandleAsync(GetPendingGroupInvitationsQuery request, CancellationToken cancellationToken)
    {
        var userInvitations = await applicationDataContext.UserGarbageGroups
            .Where(x => x.UserId == request.UserId && x.IsPending)
            .Select(x => new GarbageGroupInvitationDto
            {
                GroupId = x.GarbageGroupId,
                GroupName = x.GarbageGroup.Name,
                City = x.GarbageGroup.City
            })
            .ToListAsync(cancellationToken);
        
        foreach (var invitation in userInvitations)
        {
            var invitingUser = await applicationDataContext.UserGarbageGroups
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.GarbageGroupId == invitation.GroupId 
                                     && x.Role == Shared.Enums.GarbageGroupRole.Owner, cancellationToken);
            
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