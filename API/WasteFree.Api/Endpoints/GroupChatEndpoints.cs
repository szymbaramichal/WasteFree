using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using WasteFree.Application.Abstractions.Messaging;
using WasteFree.Application.Features.GroupChat;
using WasteFree.Application.Features.GroupChat.Dtos;
using WasteFree.Domain.Constants;
using WasteFree.Domain.Interfaces;
using WasteFree.Domain.Models;

namespace WasteFree.Api.Endpoints;

public static class GroupChatEndpoints
{
    public static void MapGroupChatEndpoints(this WebApplication app)
    {
        app.MapGet("/garbage-groups/{groupId:guid}/chat/messages", GetGroupChatMessagesAsync)
            .RequireAuthorization(PolicyNames.GenericPolicy)
            .WithOpenApi()
            .Produces<PaginatedResult<ICollection<GroupChatMessageDto>>>()
            .Produces<Result<EmptyResult>>(403)
            .Produces<Result<EmptyResult>>(404)
            .WithTags("GroupChat")
            .WithDescription("Get chat history for a garbage group.");

        app.MapPost("/garbage-groups/{groupId:guid}/chat/messages", CreateGroupChatMessageAsync)
            .RequireAuthorization(PolicyNames.GenericPolicy)
            .WithOpenApi()
            .Produces<Result<GroupChatMessageDto>>()
            .Produces<Result<EmptyResult>>(400)
            .Produces<Result<EmptyResult>>(403)
            .Produces<Result<EmptyResult>>(404)
            .WithTags("GroupChat")
            .WithDescription("Post a chat message to a garbage group.");
    }

    private static async Task<IResult> GetGroupChatMessagesAsync(
        [FromRoute] Guid groupId,
        [FromQuery] int pageNumber,
        [FromQuery] int pageSize,
        ICurrentUserService currentUserService,
        IMediator mediator,
        IStringLocalizer localizer,
        CancellationToken cancellationToken)
    {
        var pager = new Pager(pageNumber <= 0 ? 1 : pageNumber, pageSize <= 0 ? 50 : pageSize);
        var query = new GetGroupChatMessagesQuery(currentUserService.UserId, groupId, pager);

        var result = await mediator.SendAsync(query, cancellationToken);

        if (!result.IsValid)
        {
            result.ErrorMessage = localizer[$"{result.ErrorCode}"];
            return Results.Json(result, statusCode: (int)result.ResponseCode);
        }

        return Results.Ok(result);
    }

    private static async Task<IResult> CreateGroupChatMessageAsync(
        [FromRoute] Guid groupId,
        [FromBody] CreateGroupChatMessageRequest request,
        ICurrentUserService currentUserService,
        IMediator mediator,
        IStringLocalizer localizer,
        CancellationToken cancellationToken)
    {
        var command = new CreateGroupChatMessageCommand(currentUserService.UserId, groupId, request.Content);

        var result = await mediator.SendAsync(command, cancellationToken);

        if (!result.IsValid)
        {
            result.ErrorMessage = localizer[$"{result.ErrorCode}"];
            return Results.Json(result, statusCode: (int)result.ResponseCode);
        }

        return Results.Ok(result);
    }
}

public record CreateGroupChatMessageRequest
{
    public string Content { get; init; } = string.Empty;
}
