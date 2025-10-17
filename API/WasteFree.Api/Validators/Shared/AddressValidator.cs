using FluentValidation;
using Microsoft.Extensions.Localization;
using WasteFree.Domain.Constants;
using WasteFree.Domain.Models;

namespace WasteFree.Api.Validators.Shared;

public class AddressValidator : AbstractValidator<Address>
{
    private const string PostalCodePattern = @"^\d{2}-\d{3}$";

    public AddressValidator(IStringLocalizer localizer)
    {
        RuleFor(x => x.City)
            .NotEmpty()
            .WithMessage(localizer[ValidationErrorCodes.GroupCityRequired]);

        RuleFor(x => x.PostalCode)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(localizer[ValidationErrorCodes.GroupPostalCodeRequired])
            .Matches(PostalCodePattern)
            .WithMessage(localizer[ValidationErrorCodes.GroupPostalCodeInvalid]);

        RuleFor(x => x.Street)
            .NotEmpty()
            .WithMessage(localizer[ValidationErrorCodes.GroupStreetRequired]);
    }
}
