using WasteFree.Business.Abstractions.Messaging;
using WasteFree.Business.Features.Auth;

namespace WasteFree.App.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        app.MapPost("/auth/register", async (HttpContext context, 
                ICommandHandler<RegisterUserCommand> handler,
                CancellationToken cancellationToken) =>
        {
            var command = new RegisterUserCommand("test1", "test2");
            
            var result = await handler.Handle(command, cancellationToken);
        })
        .WithOpenApi();

        app.MapPost("/auth/login", async (HttpContext context) =>
        {
            // Placeholder for user login logic
            await context.Response.WriteAsync("Login endpoint placeholder");
        })
        .WithOpenApi();
    }
}