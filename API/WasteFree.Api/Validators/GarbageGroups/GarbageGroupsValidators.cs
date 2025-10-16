using FluentValidation;
using Microsoft.Extensions.Localization;
using WasteFree.App.Endpoints;
using WasteFree.Shared.Constants;

namespace WasteFree.App.Validators.GarbageGroups;

public class RegisterGarbageGroupRequestValidator : AbstractValidator<RegisterGarbageGroupRequest>
{
    private const string PostalCodePattern = @"^\d{2}-\d{3}$";

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
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(localizer[ValidationErrorCodes.GroupPostalCodeRequired])
            .Matches(PostalCodePattern)
            .WithMessage(localizer[ValidationErrorCodes.GroupPostalCodeInvalid]);

        RuleFor(x => x.Address)
            .NotEmpty()
            .WithMessage(localizer[ValidationErrorCodes.GroupAddressRequired]);
    }
}

public class UpdateGarbageGroupRequestValidator : AbstractValidator<UpdateGarbageGroupRequest>
{
    private const string PostalCodePattern = @"^\d{2}-\d{3}$";

    public UpdateGarbageGroupRequestValidator(IStringLocalizer localizer)
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
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(localizer[ValidationErrorCodes.GroupPostalCodeRequired])
            .Matches(PostalCodePattern)
            .WithMessage(localizer[ValidationErrorCodes.GroupPostalCodeInvalid]);

        RuleFor(x => x.Address)
            .NotEmpty()
            .WithMessage(localizer[ValidationErrorCodes.GroupAddressRequired]);
    }
}