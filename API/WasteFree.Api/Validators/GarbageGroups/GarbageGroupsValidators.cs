using FluentValidation;
using Microsoft.Extensions.Localization;
using WasteFree.Api.Endpoints;
using WasteFree.Api.Validators.Shared;
using WasteFree.Domain.Constants;

namespace WasteFree.Api.Validators.GarbageGroups;

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

        RuleFor(x => x.Address)
            .SetValidator(new AddressValidator(localizer));    
    }
}

public class UpdateGarbageGroupRequestValidator : AbstractValidator<UpdateGarbageGroupRequest>
{
    public UpdateGarbageGroupRequestValidator(IStringLocalizer localizer)
    {
        RuleFor(x => x.GroupName)
            .NotEmpty()
            .WithMessage(localizer[ValidationErrorCodes.GroupNameRequired]);

        RuleFor(x => x.GroupDescription)
            .NotEmpty()
            .WithMessage(localizer[ValidationErrorCodes.GroupDescriptionRequired]);

        RuleFor(x => x.Address)
            .SetValidator(new AddressValidator(localizer));
    }
}