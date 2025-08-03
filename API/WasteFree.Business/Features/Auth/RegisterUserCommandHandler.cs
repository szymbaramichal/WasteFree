using WasteFree.Business.Abstractions.Messaging;
using WasteFree.Infrastructure;
using WasteFree.Shared.Shared;

namespace WasteFree.Business.Features.Auth;

public record RegisterUserCommand(string Email, string Password) : ICommand;

public class RegisterUserCommandHandler(ApplicationDataContext context) : ICommandHandler<RegisterUserCommand>
{
    public Task<Result> Handle(RegisterUserCommand command, CancellationToken cancellationToken)
    {
        return null;
    }
}