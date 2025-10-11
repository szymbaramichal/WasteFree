using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using WasteFree.App.Filters;
using WasteFree.Business.Abstractions.Messaging;
using WasteFree.Business.Features.GarbageGroups;
using WasteFree.Business.Features.GarbageGroups.Dtos;
using WasteFree.Shared.Constants;
using WasteFree.Shared.Interfaces;
using WasteFree.Shared.Models;

namespace WasteFree.App.Endpoints;

public static class GarbageGroupsEndpoints
{
    public static void MapGarbageGroupsEndpoints(this WebApplication app)
    {
        app.MapPost("/garbage-groups/register", RegisterGarbageGroupAsync)
            .RequireAuthorization(PolicyNames.UserPolicy)
            .AddEndpointFilter(new ValidationFilter<RegisterGarbageGroupRequest>())
            .WithOpenApi()
            .Produces<Result<GarbageGroupDto>>()
            .Produces<Dictionary<string, string[]>>(422)
            .WithTags("GarbageGroups")
            .WithDescription("Register garbage group.");

        app.MapGet("/garbage-groups/list", GetGarbageGroupsListAsync)
            .RequireAuthorization(PolicyNames.UserPolicy)
            .WithOpenApi()
            .Produces<Result<ICollection<GarbageGroupInfoDto>>>()
            .WithTags("GarbageGroups")
            .WithDescription("Get list of garbage groups with quick info.");

        app.MapGet("/garbage-groups/pending-invitations", GetPendingGroupInvitations)
            .RequireAuthorization(PolicyNames.UserPolicy)
            .WithOpenApi()
            .Produces<Result<ICollection<GarbageGroupInvitationDto>>>()
            .WithTags("GarbageGroups")
            .WithDescription("Get list of pending invitations.");
        
        app.MapPost("/garbage-groups/{groupId:guid}/makeAction/{makeAction}", MakeActionWithInvitation)
            .RequireAuthorization(PolicyNames.UserPolicy)
            .WithOpenApi()
            .Produces<Result<ICollection<GarbageGroupInvitationDto>>>()
            .Produces<Result<EmptyResult>>(404)
            .WithTags("GarbageGroups")
            .WithDescription("Accept/decline group invitation.");
        
        app.MapGet("/garbage-groups/{groupId}", GetGarbageGroupDetailsAsync)
            .RequireAuthorization(PolicyNames.UserPolicy)
            .WithOpenApi()
            .Produces<Result<GarbageGroupDto>>()
            .Produces<Result<EmptyResult>>(403)
            .Produces<Result<EmptyResult>>(404)
            .WithTags("GarbageGroups")
            .WithDescription("Get details of garbage group.");

        app.MapDelete("/garbage-groups/{groupId:guid}/{userId:guid}", DeleteUserFromGarbageGroupAsync)
            .RequireAuthorization(PolicyNames.UserPolicy)
            .WithOpenApi()
            .Produces<Result<bool>>()
            .Produces<Result<EmptyResult>>(404)
            .WithTags("GarbageGroups")
            .WithDescription("Remove user from garbage group.");

        app.MapPost("/garbage-groups/invite", InviteUserToGarbageGroupAsync)
            .RequireAuthorization(PolicyNames.UserPolicy)
            .WithOpenApi()
            .Produces<Result<bool>>()
            .Produces<Result<EmptyResult>>(404)
            .Produces<Result<EmptyResult>>(400)
            .WithTags("GarbageGroups")
            .WithDescription("Invite existing user to garbage group.");
    }

    /// <summary>
    /// Registers a new garbage group and assigns the current user as owner.
    /// </summary>
    private static async Task<IResult> RegisterGarbageGroupAsync(
        [FromBody] RegisterGarbageGroupRequest request,
        IStringLocalizer localizer,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new RegisterGarbageGroupCommand(request.GroupName, 
            request.GroupDescription, request.City, request.PostalCode, request.Address);

        var result = await mediator.SendAsync(command, cancellationToken);

        if (!result.IsValid)
        {
            result.ErrorMessage = localizer[$"{result.ErrorCode}"];
            return Results.Json(result, statusCode: (int)result.ResponseCode);
        }

        return Results.Ok(result);
    }

    /// <summary>
    /// Retrieves the list of garbage groups the authenticated user participates in.
    /// </summary>
    private static async Task<IResult> GetGarbageGroupsListAsync(
        ICurrentUserService currentUserService,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new GetGarbageGroupsListQuery(currentUserService.UserId);

        var result = await mediator.SendAsync(command, cancellationToken);

        return Results.Ok(result);
    }
    
    /// <summary>
    /// Retrieves the list of pending group invitations.
    /// </summary>
    private static async Task<IResult> GetPendingGroupInvitations(
        ICurrentUserService currentUserService,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new GetPendingGroupInvitationsQuery(currentUserService.UserId);

        var result = await mediator.SendAsync(command, cancellationToken);

        return Results.Ok(result);
    }
    
    /// <summary>
    /// Accept/decline group invitation.
    /// </summary>
    private static async Task<IResult> MakeActionWithInvitation(
        [FromRoute] Guid groupId,
        [FromRoute] bool makeAction,
        ICurrentUserService currentUserService,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new MakeActionWithInvitationCommand(currentUserService.UserId, groupId, makeAction);

        var result = await mediator.SendAsync(command, cancellationToken);

        if (!result.IsValid)
        {
            return Results.Json(result, statusCode: (int)result.ResponseCode);
        }

        return Results.Ok(result);
    }

    /// <summary>
    /// Retrieves detailed information about a specific garbage group including members.
    /// </summary>
    private static async Task<IResult> GetGarbageGroupDetailsAsync(
        [FromRoute] Guid groupId,
        IStringLocalizer localizer,
        ICurrentUserService currentUserService,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new GetGarbageGroupDetailsQuery(currentUserService.UserId, groupId);

        var result = await mediator.SendAsync(command, cancellationToken);

        if (!result.IsValid)
        {
            result.ErrorMessage = localizer[$"{result.ErrorCode}"];
            return Results.Json(result, statusCode: (int)result.ResponseCode);
        }

        return Results.Ok(result);
    }

    /// <summary>
    /// Removes a specified user from the given garbage group.
    /// </summary>
    private static async Task<IResult> DeleteUserFromGarbageGroupAsync(
        [FromRoute] Guid userId,
        [FromRoute] Guid groupId,
        ICurrentUserService currentUserService,
        IStringLocalizer localizer,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new DeleteUserFromGroupCommand(groupId, currentUserService.UserId, userId);

        var result = await mediator.SendAsync(command, cancellationToken);

        if (!result.IsValid)
        {
            result.ErrorMessage = localizer[$"{result.ErrorCode}"];
            return Results.Json(result, statusCode: (int)result.ResponseCode);
        }

        return Results.Ok(result);
    }

    /// <summary>
    /// Sends an invitation to an existing user to join the specified garbage group.
    /// </summary>
    private static async Task<IResult> InviteUserToGarbageGroupAsync(
        [FromBody] InviteUserToGarbageGroupRequest request,
        IStringLocalizer localizer,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new InviteToGarbageGroupCommand(request.GroupId, request.UserName);

        var result = await mediator.SendAsync(command, cancellationToken);

        if (!result.IsValid)
        {
            result.ErrorMessage = localizer[$"{result.ErrorCode}"];
            return Results.Json(result, statusCode: (int)result.ResponseCode);
        }

        return Results.Ok(result);
    }
}

/// <summary>
/// Request payload for registering a new garbage group.
/// </summary>
public record RegisterGarbageGroupRequest
{
    /// <summary>
    /// Display name assigned to the garbage group.
    /// </summary>
    public string GroupName { get; init; } = string.Empty;

    /// <summary>
    /// Description shared with members about the group's purpose.
    /// </summary>
    public string GroupDescription { get; init; } = string.Empty;
    
    /// <summary>
    /// Group city, used for location-based features.
    /// </summary>
    public string City { get; init; } = string.Empty;

    /// <summary>
    /// Postal code tied to the group's address for more precise localisation.
    /// </summary>
    public string PostalCode { get; init; } = string.Empty;

    /// <summary>
    /// Street address of the group for contact or pickup details.
    /// </summary>
    public string Address { get; init; } = string.Empty;
}

/// <summary>
/// Request payload for inviting an existing user to join a garbage group.
/// </summary>
public record InviteUserToGarbageGroupRequest
{
    /// <summary>
    /// Username of the person receiving the invitation.
    /// </summary>
    public string UserName { get; init; } = string.Empty;

    /// <summary>
    /// Identifier of the garbage group the user is invited to join.
    /// </summary>
    public Guid GroupId { get; init; }
}
