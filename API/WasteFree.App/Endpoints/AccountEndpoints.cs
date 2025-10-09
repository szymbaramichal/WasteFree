using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using WasteFree.Business.Abstractions.Messaging;
using WasteFree.Business.Features.Account;
using WasteFree.Business.Features.Account.Dtos;
using WasteFree.Shared.Constants;
using WasteFree.Shared.Interfaces;
using WasteFree.Shared.Models;

namespace WasteFree.App.Endpoints;

public static class AccountEndpoints
{
    public static void MapAccountEndpoints(this WebApplication app)
    {
        app.MapPut("/user/profile", UpdateUserProfileAsync)
            .RequireAuthorization(PolicyNames.UserPolicy, PolicyNames.GarbageAdminPolicy)
            .WithOpenApi()
            .Produces<Result<ProfileDto>>()
            .Produces<Result<EmptyResult>>(400)
            .WithTags("Account")
            .WithDescription("Updates the authenticated user's profile fields: Description, BankAccountNumber and City.");
        
        app.MapGet("/user/profile", GetUserProfileAsync)
            .RequireAuthorization(PolicyNames.UserPolicy, PolicyNames.GarbageAdminPolicy)
            .WithOpenApi()
            .Produces<Result<ProfileDto>>()
            .Produces<Result<EmptyResult>>(400)
            .WithTags("Account")
            .WithDescription("Get authenticated user's profile.");

    }

    /// <summary>
    /// Updates the authenticated user's profile information.
    /// </summary>
    private static async Task<IResult> UpdateUserProfileAsync(
        [FromBody] UpdateProfileRequest request,
        ICurrentUserService currentUserService,
        IStringLocalizer localizer,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.SendAsync(new UpdateUserProfileCommand(currentUserService.UserId,
                request.Description, request.BankAccountNumber, request.City),
            cancellationToken);

        if (!result.IsValid)
        {
            result.ErrorMessage = localizer[$"{result.ErrorCode}"];
            return Results.Json(result, statusCode: (int)result.ResponseCode);
        }

        return Results.Ok(result);
    }

    /// <summary>
    /// Retrieves the profile information for the authenticated user.
    /// </summary>
    private static async Task<IResult> GetUserProfileAsync(
        ICurrentUserService currentUserService,
        IStringLocalizer localizer,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.SendAsync(new GetUserProfileQuery(currentUserService.UserId),
            cancellationToken);

        if (!result.IsValid)
        {
            result.ErrorMessage = localizer[$"{result.ErrorCode}"];
            return Results.Json(result, statusCode: (int)result.ResponseCode);
        }

        return Results.Ok(result);
    }
}

/// <summary>
/// Request payload containing profile information to update for the authenticated user.
/// </summary>
public record UpdateProfileRequest
{
    /// <summary>
    /// Short bio or description of the user.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Bank account number used for withdrawal payouts.
    /// </summary>
    public string BankAccountNumber { get; init; } = string.Empty;

    /// <summary>
    /// City in which the user resides.
    /// </summary>
    public string City { get; init; } = string.Empty;
}