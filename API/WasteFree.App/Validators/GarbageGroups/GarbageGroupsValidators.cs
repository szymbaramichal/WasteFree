using FluentValidation;
using WasteFree.App.Endpoints;

namespace WasteFree.App.Validators.GarbageGroups;

public class RegisterGarbageGroupRequestValidator : AbstractValidator<RegisterGarbageGroupRequest>
{
    public RegisterGarbageGroupRequestValidator()
    {
        RuleFor(x => x.GroupName).NotEmpty();

        RuleFor(x => x.GroupDescription).NotEmpty();
    }
}
