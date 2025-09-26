using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using WasteFree.App.Filters;
using WasteFree.Business.Abstractions.Messaging;
using WasteFree.Business.Features.GarbageGroups;
using WasteFree.Shared.Interfaces;

namespace WasteFree.App.Endpoints;

public static class GarbageGroupsEndpoints
{
    public static void MapGarbageGroupsEndpoints(this WebApplication app)
    {
        app.MapGet("/garbage-groups/register", [Authorize] async (
                [FromBody] RegisterGarbageGroupRequest request,
                IStringLocalizer localizer,
                IMediator mediator,
                CancellationToken cancellationToken) =>
        {
            var command = new RegisterGarbageGroupCommand(request.GroupName, request.GroupDescription);

            var result = await mediator.SendAsync(command, cancellationToken);

            if(!result.IsValid)
            {
                result.ErrorMessage = localizer[$"{result.ErrorCode}"];
                return Results.BadRequest(result);
            }

            return Results.Ok(result);
        })
        .RequireAuthorization()
        .AddEndpointFilter(new ValidationFilter<RegisterGarbageGroupRequest>())
        .WithOpenApi();
        
        app.MapGet("/garbage-groups/list", [Authorize] async (
                IStringLocalizer localizer,
                ICurrentUserService currentUserService,
                IMediator mediator,
                CancellationToken cancellationToken) =>
            {
                var command = new GetGarbageGroupsListQuery(currentUserService.UserId);

                var result = await mediator.SendAsync(command, cancellationToken);

                return Results.Ok(result);
            })
            .RequireAuthorization()
            .WithOpenApi();
        
        app.MapGet("/garbage-groups/{groupId}", [Authorize] async (
                [FromRoute] Guid groupId,
                IStringLocalizer localizer,
                ICurrentUserService currentUserService,
                IMediator mediator,
                CancellationToken cancellationToken) =>
            {
                var command = new GetGarbageGroupDetailsQuery(currentUserService.UserId, groupId);

                var result = await mediator.SendAsync(command, cancellationToken);

                if(!result.IsValid)
                {
                    result.ErrorMessage = localizer[$"{result.ErrorCode}"];
                    return Results.BadRequest(result);
                }

                return Results.Ok(result);
            })
            .RequireAuthorization()
            .AddEndpointFilter(new ValidationFilter<RegisterGarbageGroupRequest>())
            .WithOpenApi();

        app.MapPost("/garbage-groups/invite", async (
            [FromBody] InviteUserToGarbageGroupRequest request,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok();
        })
        .RequireAuthorization()
        .AddEndpointFilter(new ValidationFilter<LoginUserRequest>())
        .WithOpenApi();
    }
}

public record RegisterGarbageGroupRequest(string GroupName, string GroupDescription);
public record InviteUserToGarbageGroupRequest(string userName, Guid groupId);
