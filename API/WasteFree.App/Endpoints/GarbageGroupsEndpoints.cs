using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Org.BouncyCastle.Bcpg;
using WasteFree.App.Filters;
using WasteFree.Business.Abstractions.Messaging;
using WasteFree.Business.Features.GarbageGroups;
using WasteFree.Shared.Interfaces;

namespace WasteFree.App.Endpoints;

public static class GarbageGroupsEndpoints
{
    public static void MapGarbageGroupsEndpoints(this WebApplication app)
    {
        app.MapPost("/garbage-groups/register", async (
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
                    return Results.Json(result, statusCode: (int)result.ResponseCode);
                }

                return Results.Ok(result);
            })
            .RequireAuthorization()
            .AddEndpointFilter(new ValidationFilter<RegisterGarbageGroupRequest>())
            .WithOpenApi();
        
        app.MapGet("/garbage-groups/list", async (
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
        
        app.MapGet("/garbage-groups/{groupId}", async (
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
                    return Results.Json(result, statusCode: (int)result.ResponseCode);
                }

                return Results.Ok(result);
            })
            .RequireAuthorization()
            .WithOpenApi();
        
        app.MapDelete("/garbage-groups/{groupId:guid}/{userId:guid}", async (
                [FromRoute] Guid userId,
                [FromRoute] Guid groupId,
                ICurrentUserService currentUserService,
                IStringLocalizer localizer,
                IMediator mediator,
                CancellationToken cancellationToken) =>
            {
                var command = new DeleteUserFromGroupCommand(groupId, currentUserService.UserId, userId);

                var result = await mediator.SendAsync(command, cancellationToken);

                if(!result.IsValid)
                {
                    result.ErrorMessage = localizer[$"{result.ErrorCode}"];
                    return Results.Json(result, statusCode: (int)result.ResponseCode);
                }

                return Results.Ok(result);
            })
            .RequireAuthorization()
            .WithOpenApi();

        app.MapPost("/garbage-groups/invite", async (
            [FromBody] InviteUserToGarbageGroupRequest request,
            IStringLocalizer localizer,
            IMediator mediator,
            CancellationToken cancellationToken) =>
            {
                var command = new InviteToGarbageGroupCommand(request.GroupId, request.UserName);

                var result = await mediator.SendAsync(command, cancellationToken);

                if(!result.IsValid)
                {
                    result.ErrorMessage = localizer[$"{result.ErrorCode}"];
                    return Results.BadRequest(result);
                }

                return Results.Ok(result);
            })
            .RequireAuthorization()
            .WithOpenApi();
    }
}

public record RegisterGarbageGroupRequest(string GroupName, string GroupDescription);
public record InviteUserToGarbageGroupRequest(string UserName, Guid GroupId);
