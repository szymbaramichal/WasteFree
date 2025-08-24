using FluentValidation;
using Microsoft.Extensions.Localization;
using WasteFree.App.Endpoints;

namespace WasteFree.App.Validators.Auth;

public class RegisterUserRequestValidator : AbstractValidator<RegisterUserRequest>
{
    public RegisterUserRequestValidator(IStringLocalizer localizer)
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage(localizer["ERR_EMAIL_REQUIRED"])
            .EmailAddress()
            .WithMessage(localizer["ERR_EMAIL_INVALID"]);

        RuleFor(x => x.Username)
            .NotEmpty()
            .WithMessage(localizer["ERR_USERNAME_REQUIRED"]);

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage(localizer["ERR_PASSWORD_REQUIRED"])
            .MinimumLength(8)
            .WithMessage(localizer["ERR_PASSWORD_TOO_SHORT"]);
    }
}


public class LoginUserRequestValidator : AbstractValidator<LoginUserRequest>
{
    public LoginUserRequestValidator(IStringLocalizer localizer)
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .WithMessage(localizer["ERR_USERNAME_REQUIRED"]);

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage(localizer["ERR_PASSWORD_REQUIRED"]);
    }
}
