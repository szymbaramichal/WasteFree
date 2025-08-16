using Microsoft.AspNetCore.Mvc;
using WasteFree.App.Filters;
using WasteFree.Business.Abstractions.Messaging;
using WasteFree.Business.Features.Auth;
using WasteFree.Business.Features.GarbageGroups;

namespace WasteFree.App.Endpoints;

public static class GarbageGroupsEndpoints
{
    public static void MapGarbageGroupsEndpoints(this WebApplication app)
    {
        app.MapPost("/garbage-groups/register", async (
            [FromBody] RegisterGarbageGroupRequest request,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var command = new RegisterGarbageGroupCommand(request.GroupName, request.GroupDescription);

            var result = await mediator.SendAsync(command, cancellationToken);

            if (!result.IsValid)
            {
                return Results.BadRequest(result);
            }

            return Results.Ok(result);
        })
        .RequireAuthorization()
        .AddEndpointFilter(new ValidationFilter<RegisterGarbageGroupRequest>())
        .WithOpenApi();

        app.MapPost("/garbage-groups/join", async (
            [FromBody] LoginUserRequest userRequest,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var command = new LoginUserCommand(userRequest.Username, userRequest.Password);

            var result = await mediator.SendAsync(command, cancellationToken);

            if (!result.IsValid)
            {
                return Results.BadRequest(result);
            }

            return Results.Ok(result);
        })
        .RequireAuthorization()
        .AddEndpointFilter(new ValidationFilter<LoginUserRequest>())
        .WithOpenApi();

        app.MapPost("/garbage-groups/invite", async (
            [FromBody] LoginUserRequest userRequest,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var command = new LoginUserCommand(userRequest.Username, userRequest.Password);

            var result = await mediator.SendAsync(command, cancellationToken);

            if (!result.IsValid)
            {
                return Results.BadRequest(result);
            }

            return Results.Ok(result);
        })
        .RequireAuthorization()
        .AddEndpointFilter(new ValidationFilter<LoginUserRequest>())
        .WithOpenApi();
    }
}

public record RegisterGarbageGroupRequest(string GroupName, string GroupDescription);
public record InviteUserToGarbageGroupRequest(string userName, Guid groupId);
