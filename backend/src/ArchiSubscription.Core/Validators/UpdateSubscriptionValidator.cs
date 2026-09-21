using ArchiSubscription.Core.DTOs.Subscription;
using FluentValidation;

namespace ArchiSubscription.Core.Validators;

public class UpdateSubscriptionValidator : AbstractValidator<UpdateSubscriptionDto>
{
    public UpdateSubscriptionValidator()
    {
        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Invalid subscription type.")
            .When(x => x.Type.HasValue);

        RuleFor(x => x.ServiceProvider)
            .MaximumLength(200).WithMessage("Service provider name cannot exceed 200 characters.")
            .When(x => x.ServiceProvider is not null);

        RuleFor(x => x.SubscriberNumber)
            .MaximumLength(50).WithMessage("Subscriber number cannot exceed 50 characters.")
            .When(x => x.SubscriberNumber is not null);

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid subscription status.")
            .When(x => x.Status.HasValue);

        RuleFor(x => x.PaymentDayOfMonth)
            .InclusiveBetween(1, 28).WithMessage("Payment day must be between 1 and 28.")
            .When(x => x.PaymentDayOfMonth.HasValue);
    }
}

