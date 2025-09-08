using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using WasteFree.Business.Abstractions.Messaging;
using WasteFree.Business.Features.Account;
using WasteFree.Shared.Interfaces;

namespace WasteFree.App.Endpoints;

public static class AccountEndpoints
{
    public static void MapAccountEndpoints(this WebApplication app)
    {
        app.MapPut("/user/profile", [Authorize] async (
                [FromBody] UpdateProfileRequest request,
                ICurrentUserService currentUserService,
                IStringLocalizer localizer,
                IMediator mediator,
                CancellationToken cancellationToken) =>
        {
            var result = await mediator.SendAsync(new UpdateUserProfileCommand(currentUserService.UserId, 
                    request.Description, request.BankAccountNumber), 
                cancellationToken);
            
            return Results.Ok(result);
        })
        .WithOpenApi();
    }
}

public record UpdateProfileRequest(string Description, string BankAccountNumber);