using FluentValidation;
using Microsoft.Extensions.Localization;
using WasteFree.Api.Endpoints;
using WasteFree.Domain.Constants;
using WasteFree.Domain.Enums;

namespace WasteFree.Api.Validators.GarbageGroupOrders;

public class GarbageOrderRequestValidator : AbstractValidator<GarbageOrderRequest>
{
    public GarbageOrderRequestValidator(IStringLocalizer localizer)
    {
        RuleFor(x => x.PickupOption)
            .IsInEnum()
            .WithErrorCode(ValidationErrorCodes.PickupOptionInvalid);

        RuleFor(x => x.ContainerSize)
            .NotNull()
            .When(x => x.PickupOption == PickupOption.Container)
            .WithErrorCode(ValidationErrorCodes.ContainerSizeRequired);

        RuleFor(x => x.PickupDate)
            .NotEmpty()
            .WithErrorCode(ValidationErrorCodes.PickupDateRequired)
            .GreaterThanOrEqualTo(DateTime.Today)
            .WithErrorCode(ValidationErrorCodes.PickupDateInPast);

        When(x => x.DropOffDate.HasValue, () =>
        {
            RuleFor(x => x.DropOffDate.Value)
                .GreaterThanOrEqualTo(DateTime.Today)
                .WithErrorCode(ValidationErrorCodes.DropOffDateInPast);
        });

        RuleFor(x => x.UserIds)
            .Must(x => x.Any())
            .WithErrorCode(ValidationErrorCodes.UserIdsEmpty);
    }
}