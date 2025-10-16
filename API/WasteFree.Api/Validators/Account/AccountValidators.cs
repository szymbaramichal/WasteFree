using FluentValidation;
using Microsoft.Extensions.Localization;
using WasteFree.Api.Endpoints;
using WasteFree.Api.Validators.Shared;

namespace WasteFree.Api.Validators.Account;

public class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
{
    public UpdateProfileRequestValidator(IStringLocalizer localizer)
    {
        RuleFor(x => x.Address)
            .SetValidator(new AddressValidator(localizer));  
    }
}

