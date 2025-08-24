using FluentValidation;
using Microsoft.Extensions.Localization;
using WasteFree.App.Endpoints;

namespace WasteFree.App.Validators.GarbageGroups;

public class RegisterGarbageGroupRequestValidator : AbstractValidator<RegisterGarbageGroupRequest>
{
    public RegisterGarbageGroupRequestValidator(IStringLocalizer localizer)
    {
        RuleFor(x => x.GroupName)
            .NotEmpty()
            .WithMessage(localizer["ERR_GROUP_NAME_REQUIRED"]);

        RuleFor(x => x.GroupDescription)
            .NotEmpty()
            .WithMessage(localizer["ERR_GROUP_DESCRIPTION_REQUIRED"]);
    }
}