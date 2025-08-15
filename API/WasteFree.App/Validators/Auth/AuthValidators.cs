using FluentValidation;
using WasteFree.App.Endpoints;

namespace WasteFree.App.Validators.Auth;

public class RegisterUserRequestValidator : AbstractValidator<RegisterUserRequest>
{
    public RegisterUserRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress()
            .WithMessage("'Email' is required and must be a valid email address.");
        
        RuleFor(x => x.Username).NotEmpty().WithMessage("'Username' is required.");
        
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8)
            .WithMessage("'Password' is required and must be at least 8 characters long.");
    }
}

public class LoginUserRequestValidator : AbstractValidator<LoginUserRequest>
{
    public LoginUserRequestValidator()
    {
        RuleFor(x => x.Username).NotEmpty().WithMessage("'Username' is required.");
        
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8)
            .WithMessage("'Password' is required and must be at least 8 characters long.");
    }
}