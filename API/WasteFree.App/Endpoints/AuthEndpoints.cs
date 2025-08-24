using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using WasteFree.App.Filters;
using WasteFree.Business.Abstractions.Messaging;
using WasteFree.Business.Features.Auth;

namespace WasteFree.App.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        app.MapPost("/auth/register", async (
                [FromBody] RegisterUserRequest request,
                IStringLocalizer localizer,
                IMediator mediator,
                CancellationToken cancellationToken) =>
        {
            var command = new RegisterUserCommand(request.Email, request.Username, request.Password);
            
            var result = await mediator.SendAsync(command, cancellationToken);
            
            if(!result.IsValid)
            {
                result.ErrorMessage = localizer[$"{result.ErrorCode}"];
                return Results.BadRequest(result);
            }
            
            return Results.Ok(result);
        })
        .AddEndpointFilter(new ValidationFilter<RegisterUserRequest>())
        .WithOpenApi();

        app.MapPost("/auth/login", async (
                [FromBody] LoginUserRequest userRequest,
                IStringLocalizer localizer,
                IMediator mediator,
                CancellationToken cancellationToken) =>
        {
            var command = new LoginUserCommand(userRequest.Username, userRequest.Password);
            
            var result = await mediator.SendAsync(command, cancellationToken);
            
            if(!result.IsValid)
            {
                result.ErrorMessage = localizer[$"{result.ErrorCode}"];
                return Results.BadRequest(result);
            }

            return Results.Ok(result);
        })
        .AddEndpointFilter(new ValidationFilter<LoginUserRequest>())
        .WithOpenApi();
    }
}

public record RegisterUserRequest(string Username, string Email, string Password);

public record LoginUserRequest(string Username, string Password);
