using System;
using System.Linq;
using FluentValidation;
using Microsoft.Extensions.Localization;
using WasteFree.Api.Endpoints;
using WasteFree.Domain.Constants;
using WasteFree.Domain.Enums;

namespace WasteFree.Api.Validators.GarbageOrders;

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

        RuleFor(x => x.DropOffDate)
            .Must(dropOff => !dropOff.HasValue || dropOff.Value >= DateTime.Today)
            .WithErrorCode(ValidationErrorCodes.DropOffDateInPast);

        RuleFor(x => x.UserIds)
            .Must(x => x.Any())
            .WithErrorCode(ValidationErrorCodes.UserIdsEmpty);
    }
}

public class GarbageOrderCalculationRequestValidator : AbstractValidator<GarbageOrderCalculationRequest>
{
    public GarbageOrderCalculationRequestValidator()
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

        RuleFor(x => x.DropOffDate)
            .Must(dropOff => !dropOff.HasValue || dropOff.Value >= DateTime.Today)
            .WithErrorCode(ValidationErrorCodes.DropOffDateInPast);
    }
}

public class GarbageOrderPaymentRequestValidator : AbstractValidator<GarbageOrderPaymentRequest>
{
    public GarbageOrderPaymentRequestValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithErrorCode(ValidationErrorCodes.AmountOutsideRange);
    }
}
