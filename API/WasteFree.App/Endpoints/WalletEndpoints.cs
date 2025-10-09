using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using WasteFree.App.Filters;
using WasteFree.Business.Abstractions.Messaging;
using WasteFree.Business.Features.Wallet;
using WasteFree.Business.Features.Wallet.Dtos;
using WasteFree.Shared.Constants;
using WasteFree.Shared.Interfaces;
using WasteFree.Shared.Models;

namespace WasteFree.App.Endpoints;

public static class WalletEndpoints
{
    public static void MapWalletEndpoints(this WebApplication app)
    {
        app.MapGet("/wallet/methods", GetWalletMethodsAsync)
            .RequireAuthorization(PolicyNames.UserPolicy, PolicyNames.GarbageAdminPolicy)
            .CacheOutput(c => c.Expire(TimeSpan.FromSeconds(60)).Tag("wallet_methods"))
            .WithOpenApi()
            .Produces<Result<IReadOnlyCollection<WalletMethod>>>()
            .WithTags("Wallet")
            .WithDescription("Get all wallet available methods.");

        app.MapPost("/wallet/transaction", WalletTransactionAsync)
            .RequireAuthorization(PolicyNames.UserPolicy, PolicyNames.GarbageAdminPolicy)
            .AddEndpointFilter(new ValidationFilter<WalletTransactionRequest>())
            .WithOpenApi()
            .Produces<Result<PaymentTransactionDto>>()
            .Produces<Result<EmptyResult>>(400)
            .WithTags("Wallet")
            .WithDescription("Make transaction, deposit or withdrawal.");
        
        app.MapPost("/wallet/balance", GetWalletBalanceAsync)
            .RequireAuthorization(PolicyNames.UserPolicy, PolicyNames.GarbageAdminPolicy)
            .WithOpenApi()
            .Produces<Result<BalanceDto>>()
            .Produces<Result<EmptyResult>>(400)
            .WithTags("Wallet")
            .WithDescription("Get user current balance.");
    }

    /// <summary>
    /// Retrieves the list of supported wallet payment methods.
    /// </summary>
    private static async Task<IResult> GetWalletMethodsAsync(
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.SendAsync(new GetWalletMethodsQuery(), cancellationToken);

        if (!result.IsValid)
        {
            return Results.Json(result, statusCode: (int)result.ResponseCode);
        }

        return Results.Ok(result);
    }

    /// <summary>
    /// Executes a wallet transaction such as a deposit or withdrawal.
    /// </summary>
    private static async Task<IResult> WalletTransactionAsync(
        [FromBody] WalletTransactionRequest paymentRequest,
        ICurrentUserService currentUserService,
        IStringLocalizer localizer,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;

        var command = new WalletTransactionCommand(userId, paymentRequest.Code, paymentRequest.Amount,
            paymentRequest.PaymentProperty);

        var result = await mediator.SendAsync(command, cancellationToken);

        if (!result.IsValid)
        {
            result.ErrorMessage = localizer[$"{result.ErrorCode}"];
            return Results.Json(result, statusCode: (int)result.ResponseCode);
        }

        return Results.Ok(result);
    }

    /// <summary>
    /// Retrieves the wallet balance for the authenticated user.
    /// </summary>
    private static async Task<IResult> GetWalletBalanceAsync(
        IStringLocalizer localizer,
        ICurrentUserService currentUserService,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;

        var result = await mediator.SendAsync(new GetWalletBalanceQuery(userId), cancellationToken);

        if (!result.IsValid)
        {
            result.ErrorMessage = localizer[$"{result.ErrorCode}"];
            return Results.Json(result, statusCode: (int)result.ResponseCode);
        }

        return Results.Ok(result);
    }
}

/// <summary>
/// Request payload describing a wallet transaction to execute.
/// </summary>
public record WalletTransactionRequest
{
    /// <summary>
    /// Identifier of the payment method being used.
    /// </summary>
    public string Code { get; init; } = string.Empty;

    /// <summary>
    /// Monetary amount for the transaction.
    /// </summary>
    public double Amount { get; init; }

    /// <summary>
    /// Additional information required by the payment provider (e.g. voucher code).
    /// </summary>
    public string PaymentProperty { get; init; } = string.Empty;
}