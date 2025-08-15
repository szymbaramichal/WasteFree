using Microsoft.AspNetCore.Mvc;
using WasteFree.Business.Abstractions.Messaging;
using WasteFree.Business.Features;
using WasteFree.Business.Features.Auth;
using WasteFree.Shared.Shared;

namespace WasteFree.App.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        app.MapPost("/auth/register", async (
                [FromBody] RegisterUserRequest request,
                IMediator mediator,
                CancellationToken cancellationToken) =>
        {
            var command = new RegisterUserCommand(request.Email, request.Username, request.Password);
            
            var result = await mediator.SendAsync(command, cancellationToken);
            
            if(!result.IsValid)
            {
                return Results.BadRequest(result);
            }
            
            return Results.Ok(result);
        })
        .WithOpenApi();

        app.MapPost("/auth/login", async (
                [FromBody] LoginRequest request,
                IMediator mediator,
                CancellationToken cancellationToken) =>
        {
            var command = new LoginUserCommand(request.Username, request.Password);
            
            var result = await mediator.SendAsync(command, cancellationToken);
            
            if(!result.IsValid)
            {
                return Results.BadRequest(result);
            }

            return Results.Ok(result);
        })
        .WithOpenApi();
    }
}

public record RegisterUserRequest(string Username, string Email, string Password);

public record LoginRequest(string Username, string Password);
