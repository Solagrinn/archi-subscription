using ArchiSubscription.Core.DTOs.Subscription;
using FluentValidation;

namespace ArchiSubscription.Core.Validators;

public class CreateSubscriptionValidator : AbstractValidator<CreateSubscriptionDto>
{
    public CreateSubscriptionValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required.");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Invalid subscription type.");

        RuleFor(x => x.ServiceProvider)
            .NotEmpty().WithMessage("Service provider name is required.")
            .MaximumLength(200).WithMessage("Service provider name cannot exceed 200 characters.");

        RuleFor(x => x.SubscriberNumber)
            .NotEmpty().WithMessage("Subscriber number is required.")
            .MaximumLength(50).WithMessage("Subscriber number cannot exceed 50 characters.");

        RuleFor(x => x.PaymentDayOfMonth)
            .InclusiveBetween(1, 28).WithMessage("Payment day must be between 1 and 28.");
    }
}

