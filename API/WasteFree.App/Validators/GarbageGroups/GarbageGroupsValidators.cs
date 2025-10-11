using FluentValidation;
using Microsoft.Extensions.Localization;
using WasteFree.App.Endpoints;
using WasteFree.Shared.Constants;

namespace WasteFree.App.Validators.GarbageGroups;

public class RegisterGarbageGroupRequestValidator : AbstractValidator<RegisterGarbageGroupRequest>
{
    public RegisterGarbageGroupRequestValidator(IStringLocalizer localizer)
    {
        RuleFor(x => x.GroupName)
            .NotEmpty()
            .WithMessage(localizer[ValidationErrorCodes.GroupNameRequired]);

        RuleFor(x => x.GroupDescription)
            .NotEmpty()
            .WithMessage(localizer[ValidationErrorCodes.GroupDescriptionRequired]);

        RuleFor(x => x.City)
            .NotEmpty()
            .WithMessage(localizer[ValidationErrorCodes.GroupCityRequired]);

        RuleFor(x => x.PostalCode)
            .NotEmpty()
            .WithMessage(localizer[ValidationErrorCodes.GroupPostalCodeRequired]);

        RuleFor(x => x.Address)
            .NotEmpty()
            .WithMessage(localizer[ValidationErrorCodes.GroupAddressRequired]);
    }
}