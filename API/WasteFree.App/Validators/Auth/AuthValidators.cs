using FluentValidation;
using Microsoft.Extensions.Localization;
using WasteFree.App.Endpoints;
using WasteFree.Shared.Constants;
using WasteFree.Shared.Enums;

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
        
        
        RuleFor(x => x.Role)
            .Must(role => role.Equals(nameof(UserRole.User), StringComparison.InvariantCultureIgnoreCase) || 
                          role.Equals(nameof(UserRole.GarbageAdmin), StringComparison.InvariantCultureIgnoreCase))
            .WithMessage(localizer[ValidationErrorCodes.InvalidRole]);
        
        RuleFor(x => x.LanguagePreference)
            .Must(lang => lang.Equals(nameof(LanguagePreference.English), StringComparison.InvariantCultureIgnoreCase) || 
                          lang.Equals(nameof(LanguagePreference.Polish), StringComparison.InvariantCultureIgnoreCase))
            .WithMessage(localizer[ValidationErrorCodes.InvalidRole]);
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
