using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using WasteFree.App.Filters;
using WasteFree.Business.Abstractions.Messaging;
using WasteFree.Business.Features.Wallet;
using WasteFree.Shared.Interfaces;

namespace WasteFree.App.Endpoints;

public static class WalletEndpoints
{
    public static void MapWalletEndpoints(this WebApplication app)
    {
        app.MapGet("/wallet/methods", async (
                IStringLocalizer localizer,
                IMediator mediator,
                CancellationToken cancellationToken) =>
            {
                var result = await mediator.SendAsync(new GetWalletMethodsQuery(), cancellationToken);
                
                return Results.Ok(result);
            })
            .RequireAuthorization()
            .CacheOutput(c => c.Expire(TimeSpan.FromSeconds(60)).Tag("wallet_methods"))
            .WithOpenApi();

        app.MapPost("/wallet/transaction", async (
                [FromBody] WalletTransactionRequest paymentRequest,
                ICurrentUserService currentUserService,
                IStringLocalizer localizer,
                IMediator mediator,
                CancellationToken cancellationToken) =>
            {
                var userId = currentUserService.UserId;
                
                var command = new WalletTransactionCommand(userId, paymentRequest.Code, paymentRequest.Amount, 
                    paymentRequest.PaymentProperty);
            
                var result = await mediator.SendAsync(command, cancellationToken);
            
                if(!result.IsValid)
                {
                    result.ErrorMessage = localizer[$"{result.ErrorCode}"];
                    return Results.BadRequest(result);
                }

                return Results.Ok(result);
            })
            .RequireAuthorization()
            .AddEndpointFilter(new ValidationFilter<WalletTransactionRequest>())
            .WithOpenApi();
        
        app.MapPost("/wallet/balance", async (
                IStringLocalizer localizer,
                ICurrentUserService currentUserService,
                IMediator mediator,
                CancellationToken cancellationToken) =>
            {
                var userId = currentUserService.UserId;
            
                var result = await mediator.SendAsync(new GetWalletBalanceQuery(userId), cancellationToken);
                
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

public record WalletTransactionRequest(string Code, double Amount, string PaymentProperty);