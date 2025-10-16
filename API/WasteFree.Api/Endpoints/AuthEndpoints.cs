using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using WasteFree.Api.Filters;
using WasteFree.Application.Abstractions.Messaging;
using WasteFree.Application.Features;
using WasteFree.Application.Features.Auth;
using WasteFree.Application.Features.Auth.Dtos;
using WasteFree.Domain.Models;
using WasteFree.Domain.Models;

namespace WasteFree.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        app.MapPost("/auth/register", RegisterUserAsync)
            .AddEndpointFilter(new ValidationFilter<RegisterUserRequest>())
            .WithOpenApi()
            .Produces<Result<UserDto>>()
            .Produces<Result<EmptyResult>>(400)
            .Produces<Dictionary<string, string[]>>(422)
            .WithTags("Auth")
            .WithDescription("Register user and send confirmation mail.");

        app.MapPost("/auth/login", LoginUserAsync)
            .AddEndpointFilter(new ValidationFilter<LoginUserRequest>())
            .WithOpenApi()
            .Produces<Result<UserDto>>()
            .Produces<Result<EmptyResult>>(400)
            .Produces<Dictionary<string, string[]>>(422)
            .WithTags("Auth")
            .WithDescription("Login user and receive JWT token to authenticate.");
        
        app.MapPost("/auth/activate-account", ActivateAccountAsync)
            .WithOpenApi()
            .Produces<Result<ActivateAccountDto>>()
            .Produces<Result<EmptyResult>>(400)
            .WithTags("Auth")
            .WithDescription("Activate account after clicking link sent on email.");
    }

    /// <summary>
    /// Registers a new user account using the provided credentials and role information.
    /// </summary>
    private static async Task<IResult> RegisterUserAsync(
        [FromBody] RegisterUserRequest request,
        IStringLocalizer localizer,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new RegisterUserCommand(request.Email, request.Username, request.Password,
            request.Role, request.LanguagePreference, request.Address);

        var result = await mediator.SendAsync(command, cancellationToken);

        if (!result.IsValid)
        {
            result.ErrorMessage = localizer[$"{result.ErrorCode}"];
            return Results.Json(result, statusCode: (int)result.ResponseCode);
        }

        return Results.Ok(result);
    }

    /// <summary>
    /// Login and get access token on success.
    /// </summary>
    private static async Task<IResult> LoginUserAsync(
        [FromBody] LoginUserRequest userRequest,
        IStringLocalizer localizer,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new LoginUserCommand(userRequest.Username, userRequest.Password);

        var result = await mediator.SendAsync(command, cancellationToken);

        if (!result.IsValid)
        {
            result.ErrorMessage = localizer[$"{result.ErrorCode}"];
            return Results.Json(result, statusCode: (int)result.ResponseCode);
        }

        return Results.Ok(result);
    }

    /// <summary>
    /// Activates a user account using an encrypted activation token.
    /// </summary>
    private static async Task<IResult> ActivateAccountAsync(
        [FromQuery] string token,
        IStringLocalizer localizer,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.SendAsync(new ActivateAccountCommand(token), cancellationToken);

        if (!result.IsValid)
        {
            result.ErrorMessage = localizer[$"{result.ErrorCode}"];
            return Results.Json(result, statusCode: (int)result.ResponseCode);
        }

        return Results.Ok(result);
    }
}

/// <summary>
/// Request payload used to register a new user account.
/// </summary>
public record RegisterUserRequest
{
    /// <summary>
    /// Username selected by the new user.
    /// </summary>
    public string Username { get; init; } = string.Empty;

    /// <summary>
    /// Email address for account verification and notifications.
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Plain text password supplied during registration.
    /// </summary>
    public string Password { get; init; } = string.Empty;

    /// <summary>
    /// Role assigned to the user (e.g. Admin, User).
    /// </summary>
    public string Role { get; init; } = string.Empty;

    /// <summary>
    /// Preferred language code for localized communication.
    /// </summary>
    public string LanguagePreference { get; init; } = string.Empty;
    
    /// <summary>
    /// Address object
    /// </summary>
    public required Address Address { get; set; }
}

/// <summary>
/// Request payload used to authenticate an existing user.
/// </summary>
public record LoginUserRequest
{
    /// <summary>
    /// Username associated with the account.
    /// </summary>
    public string Username { get; init; } = string.Empty;

    /// <summary>
    /// Password provided for authentication.
    /// </summary>
    public string Password { get; init; } = string.Empty;
}
