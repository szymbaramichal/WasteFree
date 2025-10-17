using FluentValidation;
using Microsoft.Extensions.Localization;
using WasteFree.Api.Endpoints;
using WasteFree.Domain.Constants;

namespace WasteFree.Api.Validators.Wallet;

public class MakePaymentRequestValidator : AbstractValidator<WalletTransactionRequest>
{
    public MakePaymentRequestValidator(IStringLocalizer localizer)
    {
        RuleFor(x => x.Amount)
            .GreaterThan(1)
            .WithMessage(localizer[ValidationErrorCodes.AmountOutsideRange])
            .LessThan(10_000)
            .WithMessage(localizer[ValidationErrorCodes.AmountOutsideRange]);

        RuleFor(x => x.Code)
            .NotEmpty()
            .WithMessage(localizer[ValidationErrorCodes.PaymentCodeRequired])
            .Must(code => WalletMethods.AvailableMethods
                .Any(m => string.Equals(m.Code, code, StringComparison.InvariantCultureIgnoreCase)))
            .WithMessage(localizer[ValidationErrorCodes.InvalidPaymentCode]);
    }
}
