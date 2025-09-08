using Microsoft.EntityFrameworkCore;
using WasteFree.Business.Abstractions.Messaging;
using WasteFree.Business.Features.GarbageGroups.Dtos;
using WasteFree.Infrastructure;
using WasteFree.Shared.Entities;
using WasteFree.Shared.Enums;
using WasteFree.Shared.Interfaces;
using WasteFree.Shared.Models;

namespace WasteFree.Business.Features.GarbageGroups;

public record RegisterGarbageGroupCommand(string GroupName, string GroupDescription) : IRequest<GarbageGroupDto>;

public class RegisterGarbageGroupCommandHandler(ApplicationDataContext context,
    ICurrentUserService currentUserService) : IRequestHandler<RegisterGarbageGroupCommand, GarbageGroupDto>
{
    public async Task<Result<GarbageGroupDto>> HandleAsync(RegisterGarbageGroupCommand request, CancellationToken cancellationToken)
    {
        var garbageGroup = new GarbageGroup
        {
            Id = Guid.CreateVersion7(),
            Name = request.GroupName,
            Description = request.GroupDescription
        };

        context.Add(garbageGroup);

        Guid userGarbageGroupId = Guid.CreateVersion7();
        context.Add(new UserGarbageGroup
        {
            Id = userGarbageGroupId,
            UserId = currentUserService.UserId,
            GarbageGroupId = garbageGroup.Id,
            Role = GarbageGroupRole.Owner
        });

        await context.SaveChangesAsync();

        var userGarbageGroup = await context.UserGarbageGroups
            .Include(x => x.User)
            .FirstAsync(x => x.Id == userGarbageGroupId);

        return Result<GarbageGroupDto>.Success(garbageGroup.MapToGarbageGroupDto([userGarbageGroup]));
    }
}