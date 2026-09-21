using ArchiSubscription.Core.DTOs.Payment;
using FluentValidation;

namespace ArchiSubscription.Core.Validators;

public class CreatePaymentValidator : AbstractValidator<CreatePaymentDto>
{
    public CreatePaymentValidator()
    {
        RuleFor(x => x.SubscriptionId)
            .NotEmpty().WithMessage("Subscription ID is required.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Payment amount must be greater than zero.");

        RuleFor(x => x.Period)
            .NotEmpty().WithMessage("Payment period is required.")
            .Matches(@"^\d{4}-(0[1-9]|1[0-2])$").WithMessage("Period must be in format 'YYYY-MM' (e.g., 2026-05).");
    }
}

