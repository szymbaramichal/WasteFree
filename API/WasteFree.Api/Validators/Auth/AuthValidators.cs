using FluentValidation;
using Microsoft.Extensions.Localization;
using WasteFree.Api.Endpoints;
using WasteFree.Api.Validators.Shared;
using WasteFree.Domain.Constants;
using WasteFree.Domain.Enums;

namespace WasteFree.Api.Validators.Auth;

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
            .WithMessage(localizer[ValidationErrorCodes.PasswordTooShort]);
        
        RuleFor(x => x.Address)
            .SetValidator(new AddressValidator(localizer));  
        
        RuleFor(x => x.Role)
            .Must(role => role.Equals(nameof(UserRole.User), StringComparison.InvariantCultureIgnoreCase) || 
                          role.Equals(nameof(UserRole.GarbageAdmin), StringComparison.InvariantCultureIgnoreCase))
            .WithMessage(localizer[ValidationErrorCodes.InvalidRole]);
        
        RuleFor(x => x.LanguagePreference)
            .Must(lang => lang.Equals(nameof(LanguagePreference.English), StringComparison.InvariantCultureIgnoreCase) || 
                          lang.Equals(nameof(LanguagePreference.Polish), StringComparison.InvariantCultureIgnoreCase))
            .WithMessage(localizer[ValidationErrorCodes.InvalidLang]);

        When(x => x.Role.Equals(nameof(UserRole.GarbageAdmin), StringComparison.InvariantCultureIgnoreCase), () =>
        {
            RuleFor(x => x.PickupOptions)
                .NotNull()
                .WithMessage(localizer[ValidationErrorCodes.PickupOptionsRequired])
                .Must(options => options!.Any())
                .WithMessage(localizer[ValidationErrorCodes.PickupOptionsRequired]);

            RuleForEach(x => x.PickupOptions)
                .Must(option => Enum.IsDefined(typeof(PickupOption), option))
                .WithMessage(localizer[ValidationErrorCodes.PickupOptionInvalid]);
        });
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
