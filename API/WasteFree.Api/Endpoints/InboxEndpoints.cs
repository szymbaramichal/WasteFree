using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using WasteFree.Business.Abstractions.Messaging;
using WasteFree.Business.Features.Inbox;
using WasteFree.Business.Features.Inbox.Dtos;
using WasteFree.Shared.Constants;
using WasteFree.Shared.Interfaces;
using WasteFree.Shared.Models;

namespace WasteFree.App.Endpoints;

public static class InboxEndpoints
{
    public static void MapInboxEndpoints(this WebApplication app)
    {
        app.MapGet("/inbox/counter", GetInboxCounterAsync)
            .RequireAuthorization(PolicyNames.GenericPolicy)
            .WithOpenApi()
            .Produces<Result<InboxCounterDto>>()
            .WithTags("Inbox")
            .WithDescription("Get inbox messages counter.");
        
        app.MapPost("/inbox/messages/{id}/action/{makeAction}", MakeInboxMessageActionAsync)
            .RequireAuthorization(PolicyNames.GenericPolicy)
            .WithOpenApi()
            .Produces<Result<bool>>()
            .Produces<Result<EmptyResult>>(404)
            .Produces<Result<EmptyResult>>(400)
            .WithTags("Inbox")
            .WithDescription("Make action by accepting or declining action. Action can be various like joining group, submitting expense etc.");

        app.MapDelete("/inbox/{messageId:guid}", DeleteInboxMessageAsync)
            .RequireAuthorization(PolicyNames.GenericPolicy)
            .WithOpenApi()
            .Produces<Result<bool>>()
            .Produces<Result<EmptyResult>>(404)
            .WithTags("Inbox")
            .WithDescription("Remove message from inbox");
            
        app.MapPost("/inbox/clear", ClearInboxMessageAsync)
            .RequireAuthorization(PolicyNames.GenericPolicy)
            .WithOpenApi()
            .Produces<Result<bool>>()
            .WithTags("Inbox")
            .WithDescription("Clear inbox");
        
        app.MapGet("/inbox/messages", GetInboxMessagesAsync)
            .RequireAuthorization(PolicyNames.GenericPolicy)
            .WithOpenApi()
            .Produces<PaginatedResult<ICollection<InboxMessageDto>>>()
            .WithTags("Inbox")
            .WithDescription("Get all messages from inbox. Endpoint supports pagination");
    }

    /// <summary>
    /// Retrieves the number of unread inbox messages for the authenticated user.
    /// </summary>
    private static async Task<IResult> GetInboxCounterAsync(
        ICurrentUserService currentUserService,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.SendAsync(new GetInboxCounterQuery(currentUserService.UserId), cancellationToken);

        if (!result.IsValid)
        {
            return Results.Json(result, statusCode: (int)result.ResponseCode);
        }

        return Results.Ok(result);
    }

    /// <summary>
    /// Applies an action (accept/decline) to the specified inbox message.
    /// </summary>
    private static async Task<IResult> MakeInboxMessageActionAsync(
        [FromRoute] Guid id,
        [FromRoute] bool makeAction,
        ICurrentUserService currentUserService,
        IStringLocalizer localizer,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.SendAsync(
            new MakeInboxMessageActionCommand(currentUserService.UserId, id, makeAction),
            cancellationToken);

        if (!result.IsValid)
        {
            result.ErrorMessage = localizer[$"{result.ErrorCode}"];
            return Results.Json(result, statusCode: (int)result.ResponseCode);
        }

        return Results.Ok(result);
    }

    /// <summary>
    /// Deletes a specific inbox message for the authenticated user.
    /// </summary>
    private static async Task<IResult> DeleteInboxMessageAsync(
        [FromRoute] Guid messageId,
        ICurrentUserService currentUserService,
        IStringLocalizer localizer,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.SendAsync(
            new DeleteInboxMessageComamnd(currentUserService.UserId, messageId),
            cancellationToken);

        if (!result.IsValid)
        {
            result.ErrorMessage = localizer[$"{result.ErrorCode}"];
            return Results.Json(result, statusCode: (int)result.ResponseCode);
        }

        return Results.Ok(result);
    }
    
    /// <summary>
    /// Deletes all users inbox messages.
    /// </summary>
    private static async Task<IResult> ClearInboxMessageAsync(
        ICurrentUserService currentUserService,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.SendAsync(
            new ClearInboxCommand(currentUserService.UserId),
            cancellationToken);

        return Results.Ok(result);
    }

    /// <summary>
    /// Retrieves paginated inbox messages for the authenticated user.
    /// </summary>
    private static async Task<IResult> GetInboxMessagesAsync(
        [FromQuery] int pageNumber,
        [FromQuery] int pageSize,
        ICurrentUserService currentUserService,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.SendAsync(
            new GetInboxMessagesQuery(currentUserService.UserId, new Pager(pageNumber, pageSize)),
            cancellationToken);

        if (!result.IsValid)
        {
            return Results.Json(result, statusCode: (int)result.ResponseCode);
        }

        return Results.Ok(result);
    }
}