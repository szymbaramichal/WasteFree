using System.Net;
using Microsoft.EntityFrameworkCore;
using WasteFree.Business.Abstractions.Messaging;
using WasteFree.Business.Features.GarbageGroups.Dtos;
using WasteFree.Infrastructure;
using WasteFree.Infrastructure.Extensions;
using WasteFree.Shared.Constants;
using WasteFree.Shared.Enums;
using WasteFree.Shared.Models;

namespace WasteFree.Business.Features.GarbageGroups;

public record UpdateGarbageGroupCommand(Guid GroupId,
    Guid CurrentUserId,
    string GroupName,
    string GroupDescription,
    string City,
    string PostalCode,
    string Address) : IRequest<GarbageGroupDto>;

public class UpdateGarbageGroupCommandHandler(ApplicationDataContext context) 
    : IRequestHandler<UpdateGarbageGroupCommand, GarbageGroupDto>
{
    public async Task<Result<GarbageGroupDto>> HandleAsync(UpdateGarbageGroupCommand request, CancellationToken cancellationToken)
    {
        var garbageGroup = await context.GarbageGroups
            .FilterNonPrivate()
            .Include(g => g.UserGarbageGroups)
            .ThenInclude(ug => ug.User)
            .FirstOrDefaultAsync(g => g.Id == request.GroupId, cancellationToken);

        if (garbageGroup is null)
        {
            return Result<GarbageGroupDto>.Failure(ApiErrorCodes.NotFound, HttpStatusCode.NotFound);
        }

        var userMembership = garbageGroup.UserGarbageGroups.FirstOrDefault(ug => ug.UserId == request.CurrentUserId);

        if (userMembership is null || userMembership.Role != GarbageGroupRole.Owner)
        {
            return Result<GarbageGroupDto>.Failure(ApiErrorCodes.Forbidden, HttpStatusCode.Forbidden);
        }

        garbageGroup.Name = request.GroupName;
        garbageGroup.Description = request.GroupDescription;
        garbageGroup.City = request.City;
        garbageGroup.PostalCode = request.PostalCode;
        garbageGroup.Address = request.Address;

        await context.SaveChangesAsync(cancellationToken);

        return Result<GarbageGroupDto>.Success(garbageGroup.MapToGarbageGroupDto(garbageGroup.UserGarbageGroups));
    }
}