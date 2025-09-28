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
        app.MapPut("/user/profile", async (
                [FromBody] UpdateProfileRequest request,
                ICurrentUserService currentUserService,
                IStringLocalizer localizer,
                IMediator mediator,
                CancellationToken cancellationToken) =>
            {
                var result = await mediator.SendAsync(new UpdateUserProfileCommand(currentUserService.UserId, 
                        request.Description, request.BankAccountNumber), 
                    cancellationToken);
                
                if(!result.IsValid)
                {
                    result.ErrorMessage = localizer[$"{result.ErrorCode}"];
                    return Results.Json(result, statusCode: (int)result.ResponseCode);
                }
            
                return Results.Ok(result);
            })
            .RequireAuthorization()
            .WithOpenApi();
        
        app.MapGet("/user/profile", async (
                ICurrentUserService currentUserService,
                IStringLocalizer localizer,
                IMediator mediator,
                CancellationToken cancellationToken) =>
            {
                var result = await mediator.SendAsync(new GetUserProfileQuery(currentUserService.UserId), 
                    cancellationToken);
            
                if(!result.IsValid)
                {
                    result.ErrorMessage = localizer[$"{result.ErrorCode}"];
                    return Results.Json(result, statusCode: (int)result.ResponseCode);
                }
                
                return Results.Ok(result);
            })
            .RequireAuthorization()
            .WithOpenApi();
    }
}

public record UpdateProfileRequest(string Description, string BankAccountNumber);