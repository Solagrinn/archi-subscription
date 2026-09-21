using ArchiSubscription.Core.Enums;

namespace ArchiSubscription.Core.DTOs.Subscription;

public record UpdateSubscriptionDto(
    SubscriptionType? Type,
    string? ServiceProvider,
    string? SubscriberNumber,
    SubscriptionStatus? Status,
    int? PaymentDayOfMonth
);

