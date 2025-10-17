using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using WasteFree.Application.Abstractions.Messaging;
using WasteFree.Application.Features.Account;
using WasteFree.Application.Features.Account.Dtos;
using WasteFree.Domain.Constants;
using WasteFree.Domain.Interfaces;
using WasteFree.Domain.Models;

namespace WasteFree.Api.Endpoints;

public static class AccountEndpoints
{
    public static void MapAccountEndpoints(this WebApplication app)
    {
        app.MapPut("/user/profile", UpdateUserProfileAsync)
            .RequireAuthorization(PolicyNames.GenericPolicy)
            .WithOpenApi()
            .Produces<Result<ProfileDto>>()
            .Produces<Result<EmptyResult>>(400)
            .WithTags("Account")
            .WithDescription("Updates the authenticated user's profile fields: Description, BankAccountNumber and City.");

        app.MapPost("/user/avatar/upload", UploadUserAvatar)
            .RequireAuthorization(PolicyNames.GenericPolicy)
            .Accepts<UploadAvatarRequest>("multipart/form-data")
            .DisableAntiforgery()
            .WithOpenApi()
            .Produces<Result<ProfileDto>>()
            .Produces<Result<EmptyResult>>(400)
            .WithTags("Account")
            .WithDescription("Uploads or replaces the authenticated user's avatar image.");
        
        app.MapGet("/user/profile", GetUserProfileAsync)
            .RequireAuthorization(PolicyNames.GenericPolicy)
            .WithOpenApi()
            .Produces<Result<ProfileDto>>()
            .Produces<Result<EmptyResult>>(400)
            .WithTags("Account")
            .WithDescription("Get authenticated user's profile.");

        app.MapPatch("/user/consents/accept", AcceptConsentAsync)
            .RequireAuthorization(PolicyNames.GarbageAdminPolicy)
            .WithOpenApi()
            .Produces<Result<bool>>()
            .WithTags("Account")
            .WithDescription("Accept garbage admin consent.");
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
                request.Description, request.BankAccountNumber, request.Address),
            cancellationToken);

        if (!result.IsValid)
        {
            result.ErrorMessage = localizer[$"{result.ErrorCode}"];
            return Results.Json(result, statusCode: (int)result.ResponseCode);
        }

        return Results.Ok(result);
    }
    
    /// <summary>
    /// Accept garbage admin consent
    /// </summary>
    private static async Task<IResult> AcceptConsentAsync(
        ICurrentUserService currentUserService,
        IStringLocalizer localizer,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new AcceptGarbageAdminConsentCommand(currentUserService.UserId);

        var result = await mediator.SendAsync(command, cancellationToken);

        if (!result.IsValid)
        {
            result.ErrorMessage = localizer[$"{result.ErrorCode}"];
            return Results.Json(result, statusCode: (int)result.ResponseCode);
        }

        return Results.Ok(result);
    }

    
    /// <summary>
    /// Upload user avatar image.
    /// </summary>
    private static async Task<IResult> UploadUserAvatar(
        [FromForm] UploadAvatarRequest request,
        ICurrentUserService currentUserService,
        IStringLocalizer localizer,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.SendAsync(new UploadAvatarCommand(currentUserService.UserId, request.Avatar),
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
    /// Address object
    /// </summary>
    public required Address Address { get; init; }
}

/// <summary>
/// Multipart/form-data request for uploading an avatar image.
/// </summary>
public class UploadAvatarRequest
{
    /// <summary>
    /// The image file to upload as the user's avatar.
    /// </summary>
    public IFormFile Avatar { get; set; } = default!;
}
