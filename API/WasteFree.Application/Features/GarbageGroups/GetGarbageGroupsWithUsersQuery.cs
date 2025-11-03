using Microsoft.EntityFrameworkCore;
using WasteFree.Application.Abstractions.Messaging;
using WasteFree.Application.Features.GarbageGroups.Dtos;
using WasteFree.Domain.Models;
using WasteFree.Infrastructure;

namespace WasteFree.Application.Features.GarbageGroups;

public record GetGarbageGroupsWithUsersQuery(Guid UserId) : IRequest<ICollection<GarbageGroupMembersDto>>;

public class GetGarbageGroupsWithUsersQueryHandler(ApplicationDataContext context)
    : IRequestHandler<GetGarbageGroupsWithUsersQuery, ICollection<GarbageGroupMembersDto>>
{
    public async Task<Result<ICollection<GarbageGroupMembersDto>>> HandleAsync(
        GetGarbageGroupsWithUsersQuery request,
        CancellationToken cancellationToken)
    {
        var userGroups = await context.GarbageGroups
            .AsNoTracking()
            .Include(g => g.UserGarbageGroups)
            .ThenInclude(ug => ug.User)
            .Where(g => g.UserGarbageGroups.Any(ug => ug.UserId == request.UserId && !ug.IsPending))
            .ToListAsync(cancellationToken);

        var mappedGroups = userGroups
            .Select(group => new GarbageGroupMembersDto
            {
                GroupId = group.Id,
                GroupUsers = group.UserGarbageGroups.MapToGarbageGroupUserDto(),
                IsPrivate = group.IsPrivate,
                Address = group.Address
            })
            .ToList();

        return Result<ICollection<GarbageGroupMembersDto>>.Success(mappedGroups);
    }
}
