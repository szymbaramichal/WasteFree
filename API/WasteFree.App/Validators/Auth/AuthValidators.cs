using FluentValidation;
using Microsoft.Extensions.Localization;
using WasteFree.App.Endpoints;
using WasteFree.Shared.Constants;

namespace WasteFree.App.Validators.Auth;

public class RegisterUserRequestValidator : AbstractValidator<RegisterUserRequest>
{
    public RegisterUserRequestValidator(IStringLocalizer localizer)
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage(localizer[ValidationErrorCodes.EmailRequired])
            .EmailAddress()
            .WithMessage(localizer[ValidationErrorCodes.InvalidEmail]);

        RuleFor(x => x.Username)
            .NotEmpty()
            .WithMessage(localizer[ValidationErrorCodes.UsernameRequired]);

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage(localizer[ValidationErrorCodes.PasswordRequired])
            .MinimumLength(8)
            .WithMessage(localizer[ValidationErrorCodes.TooShort]);
    }
}


public class LoginUserRequestValidator : AbstractValidator<LoginUserRequest>
{
    public LoginUserRequestValidator(IStringLocalizer localizer)
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .WithMessage(localizer[ValidationErrorCodes.UsernameRequired]);

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage(localizer[ValidationErrorCodes.PasswordRequired]);
    }
}
