using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WasteFree.Business.Abstractions.Messaging;
using WasteFree.Business.Features.Inbox;
using WasteFree.Shared.Interfaces;
using WasteFree.Shared.Models;

namespace WasteFree.App.Endpoints;

public static class InboxEndpoints
{
    public static void MapInboxEndpoints(this WebApplication app)
    {
        app.MapGet("/inbox/counter", [Authorize] async (
                ICurrentUserService currentUserService,
                IMediator mediator,
                CancellationToken cancellationToken) =>
        {
            var result = await mediator.SendAsync(new GetInboxCounterQuery(currentUserService.UserId), cancellationToken);
            
            return Results.Ok(result);
        })
        .WithOpenApi();
        
        app.MapGet("/inbox/messages", [Authorize] async (
                [FromQuery] int pageNumber,
                [FromQuery] int pageSize,
                ICurrentUserService currentUserService,
                IMediator mediator,
                CancellationToken cancellationToken) =>
        {
            var result = await mediator.SendAsync(
                new GetInboxMessagesQuery(currentUserService.UserId, new Pager(pageNumber, pageSize)),
                cancellationToken);
            return Results.Ok(result);
        })
        .WithOpenApi();

    }
}